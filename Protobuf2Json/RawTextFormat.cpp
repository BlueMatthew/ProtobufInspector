#include "stdafx.h"
#include "RawTextFormat.h"
#include <stdio.h>
#include <algorithm>
#include <vector>
#include <queue>

#include <google/protobuf/port_def.inc>



// ===========================================================================

RawTextFormat::BaseTextGenerator::~BaseTextGenerator() {}

// ===========================================================================
// Internal class for writing text to the io::ZeroCopyOutputStream. Adapted
// from the Printer found in //net/proto2/io/public/printer.h
class RawTextFormat::Printer::TextGenerator
: public RawTextFormat::BaseTextGenerator {
 public:
  explicit TextGenerator(google::protobuf::io::ZeroCopyOutputStream* output,
                         int initial_indent_level)
      : output_(output),
        buffer_(nullptr),
        buffer_size_(0),
        at_start_of_line_(true),
        failed_(false),
        indent_level_(initial_indent_level),
        initial_indent_level_(initial_indent_level) {}

  ~TextGenerator() {
    // Only BackUp() if we're sure we've successfully called Next() at least
    // once.
    if (!failed_ && buffer_size_ > 0) {
      output_->BackUp(buffer_size_);
    }
  }

  // Indent text by two spaces.  After calling Indent(), two spaces will be
  // inserted at the beginning of each line of text.  Indent() may be called
  // multiple times to produce deeper indents.
  void Indent() override { ++indent_level_; }

  // Reduces the current indent level by two spaces, or crashes if the indent
  // level is zero.
  void Outdent() override {
    if (indent_level_ == 0 || indent_level_ < initial_indent_level_) {
      GOOGLE_LOG(DFATAL) << " Outdent() without matching Indent().";
      return;
    }

    --indent_level_;
  }

  // Print text to the output stream.
  void Print(const char* text, size_t size) override {
    if (indent_level_ > 0) {
      size_t pos = 0;  // The number of bytes we've written so far.
      for (size_t i = 0; i < size; i++) {
        if (text[i] == '\n') {
          // Saw newline.  If there is more text, we may need to insert an
          // indent here.  So, write what we have so far, including the '\n'.
          Write(text + pos, i - pos + 1);
          pos = i + 1;

          // Setting this true will cause the next Write() to insert an indent
          // first.
          at_start_of_line_ = true;
        }
      }
      // Write the rest.
      Write(text + pos, size - pos);
    } else {
      Write(text, size);
      if (size > 0 && text[size - 1] == '\n') {
        at_start_of_line_ = true;
      }
    }
  }

  // True if any write to the underlying stream failed.  (We don't just
  // crash in this case because this is an I/O failure, not a programming
  // error.)
  bool failed() const { return failed_; }

 private:
  GOOGLE_DISALLOW_EVIL_CONSTRUCTORS(TextGenerator);

  void Write(const char* data, size_t size) {
    if (failed_) return;
    if (size == 0) return;

    if (at_start_of_line_) {
      // Insert an indent.
      at_start_of_line_ = false;
      WriteIndent();
      if (failed_) return;
    }

    while (size > buffer_size_) {
      // Data exceeds space in the buffer.  Copy what we can and request a
      // new buffer.
      if (buffer_size_ > 0) {
        memcpy(buffer_, data, buffer_size_);
        data += buffer_size_;
        size -= buffer_size_;
      }
      void* void_buffer = nullptr;
      failed_ = !output_->Next(&void_buffer, &buffer_size_);
      if (failed_) return;
      buffer_ = reinterpret_cast<char*>(void_buffer);
    }

    // Buffer is big enough to receive the data; copy it.
    memcpy(buffer_, data, size);
    buffer_ += size;
    buffer_size_ -= size;
  }

  void WriteIndent() {
    if (indent_level_ == 0) {
      return;
    }
    GOOGLE_DCHECK(!failed_);
    int size = 2 * indent_level_;

    while (size > buffer_size_) {
      // Data exceeds space in the buffer. Write what we can and request a new
      // buffer.
      if (buffer_size_ > 0) {
        memset(buffer_, ' ', buffer_size_);
      }
      size -= buffer_size_;
      void* void_buffer;
      failed_ = !output_->Next(&void_buffer, &buffer_size_);
      if (failed_) return;
      buffer_ = reinterpret_cast<char*>(void_buffer);
    }

    // Buffer is big enough to receive the data; copy it.
    memset(buffer_, ' ', size);
    buffer_ += size;
    buffer_size_ -= size;
  }

  io::ZeroCopyOutputStream* const output_;
  char* buffer_;
  int buffer_size_;
  bool at_start_of_line_;
  bool failed_;

  int indent_level_;
  int initial_indent_level_;
};

RawTextFormat::Printer::Printer()
    : initial_indent_level_(0),
      single_line_mode_(false),
      use_short_repeated_primitives_(false),
      print_message_fields_in_index_order_(false)
      {
          line_sep_[0] = '\n';
          line_sep_[1] = '\0';
}

RawTextFormat::Printer::~Printer() {
  
}

bool RawTextFormat::Printer::PrintToString(const Message& message,
                                        std::string* output) const {
  GOOGLE_DCHECK(output) << "output specified is nullptr";

  output->clear();
  io::StringOutputStream output_stream(output);

  return Print(message, &output_stream);
}

bool RawTextFormat::Printer::PrintUnknownFieldsToString(
    const UnknownFieldSet& unknown_fields, std::string* output) const {
  GOOGLE_DCHECK(output) << "output specified is nullptr";

  output->clear();
  io::StringOutputStream output_stream(output);
  return PrintUnknownFields(unknown_fields, &output_stream);
}

bool RawTextFormat::Printer::Print(const Message& message,
                                google::protobuf::io::ZeroCopyOutputStream* output) const {
    RawTextFormat::Printer::TextGenerator generator(output, initial_indent_level_);

  Print(message, &generator);

  // Output false if the generator failed internally.
  return !generator.failed();
}

bool RawTextFormat::Printer::PrintUnknownFields(
    const UnknownFieldSet& unknown_fields,
    io::ZeroCopyOutputStream* output) const {
    RawTextFormat::Printer::TextGenerator generator(output, initial_indent_level_);

  PrintUnknownFields(unknown_fields, &generator);

  // Output false if the generator failed internally.
  return !generator.failed();
}



void RawTextFormat::Printer::Print(const Message& message,
                                TextGenerator* generator) const {
    generator->PrintString("{");
    if (!single_line_mode_) generator->PrintLiteral(line_sep_);
    generator->Indent();
    
  const Reflection* reflection = message.GetReflection();
  if (!reflection) {
    // This message does not provide any way to describe its structure.
    // Parse it again in an UnknownFieldSet, and display this instead.
    UnknownFieldSet unknown_fields;
    {
      std::string serialized = message.SerializeAsString();
      io::ArrayInputStream input(serialized.data(), serialized.size());
      unknown_fields.ParseFromZeroCopyStream(&input);
    }
    PrintUnknownFields(unknown_fields, generator);
    return;
  }
  
    PrintUnknownFields(reflection->GetUnknownFields(message), generator);
    
    if (!single_line_mode_) generator->Outdent();
    // generator->PrintLiteral(line_sep_);
    generator->PrintString("}");
    
}

struct ArrayIndexRange
{
    int number;
    int startIndex;
    int endIndex;
    
    ArrayIndexRange(int n, int s, int e) : number(n), startIndex(s), endIndex(e)
    {
    }
};

void RawTextFormat::Printer::PrintUnknownFields(
    const UnknownFieldSet& unknown_fields, TextGenerator* generator) const {
    int field_count = unknown_fields.field_count();
    int last_field_idx = field_count - 1;
    
    int prev_field_number = -1;
    int first_index = -1;
    int last_index = -1;
    
    std::queue<ArrayIndexRange*> arrayIndexes;
    // Check if there is array or map
    for (int i = 0; i < field_count; i++) {
        const UnknownField& field = unknown_fields.field(i);
        int field_number = field.number();
        if (prev_field_number == field_number)
        {
            if (first_index == -1)
            {
                first_index = i - 1;
            }
            last_index = i;
        }
        else
        {
            if (first_index != -1)
            {
                arrayIndexes.push(new ArrayIndexRange(prev_field_number, first_index, last_index));
            }
            prev_field_number = field_number;
            first_index = -1;
            last_index = -1;
        }
    }
    if (first_index != -1)
    {
        arrayIndexes.push(new ArrayIndexRange(prev_field_number, first_index, last_index));
    }
    
    ArrayIndexRange *arrayIndexRange = NULL;
    if (!arrayIndexes.empty())
    {
        arrayIndexRange = arrayIndexes.front();
        arrayIndexes.pop();
    }
    
    

  for (int i = 0; i < field_count; i++) {
    const UnknownField& field = unknown_fields.field(i);
    std::string field_number = StrCat(field.number());

      bool isArrayStart = false;
      bool isArrayEnd = false;
      
    if (arrayIndexRange != NULL)
    {
        if (i > arrayIndexRange->endIndex)
        {
            delete arrayIndexRange;
            if (!arrayIndexes.empty())
            {
                arrayIndexRange = arrayIndexes.front();
                arrayIndexes.pop();
            }
            else
            {
                arrayIndexRange = NULL;
            }
        }
    }
    
      if (arrayIndexRange != NULL && i >= arrayIndexRange->startIndex && i <= arrayIndexRange->endIndex)
      {
          if (i == arrayIndexRange->startIndex)
          {
              isArrayStart = true;
              
              generator->PrintString("\"");
              generator->PrintString(field_number);
              generator->PrintString("\"");
              generator->PrintLiteral(": [");
              
              generator->PrintLiteral(line_sep_);
              if (!single_line_mode_) {
                generator->Indent();
              }
          }
          else if (i == arrayIndexRange->endIndex)
          {
              isArrayEnd = true;
          }
      }
      else
      {
          generator->PrintString("\"");
          generator->PrintString(field_number);
          generator->PrintString("\"");
          generator->PrintLiteral(": ");
      }
      
    switch (field.type()) {
      case UnknownField::TYPE_VARINT:
        generator->PrintString(StrCat(field.varint()));
        
            if (isArrayEnd)
            {
                generator->PrintLiteral("]");
            }
        
            if (i != last_field_idx) generator->PrintLiteral(",");
        
        break;
      case UnknownField::TYPE_FIXED32: {
        generator->PrintString(
            StrCat(field.fixed32()));
          if (isArrayEnd) generator->PrintLiteral("]");
          if (i != last_field_idx) generator->PrintLiteral(",");
          
        break;
      }
      case UnknownField::TYPE_FIXED64: {
        
        generator->PrintString(StrCat(field.fixed64()));
          if (isArrayEnd) generator->PrintLiteral("]");
          if (i != last_field_idx) generator->PrintLiteral(",");
          
        break;
      }
      case UnknownField::TYPE_LENGTH_DELIMITED: {
        
        const std::string& value = field.length_delimited();
        UnknownFieldSet embedded_unknown_fields;
        if (!value.empty() && embedded_unknown_fields.ParseFromString(value)) {
          // This field is parseable as a Message.
          // So it is probably an embedded message.
          if (single_line_mode_) {
            generator->PrintLiteral("{ ");
          } else {
            generator->PrintLiteral("{\n");
            generator->Indent();
          }
            
          PrintUnknownFields(embedded_unknown_fields, generator);
            
            if (!single_line_mode_)
            {
                generator->Outdent();
            }
            generator->PrintLiteral("}");
            
            if (isArrayEnd)
            {
                generator->PrintLiteral(line_sep_);
                if (!single_line_mode_) {
                  generator->Outdent();
                }
                generator->PrintLiteral("]");
            }
            
          if (single_line_mode_) {
              if (i != last_field_idx) generator->PrintLiteral(",");
          } else {
            // generator->Outdent();
              if (i != last_field_idx) generator->PrintLiteral(",");
          }
            
            
        } else {
          // This field is not parseable as a Message.
          // So it is probably just a plain string.
          generator->PrintLiteral("\"");
          // if (this->)
            
            generator->PrintString(strings::Utf8SafeCEscape(value));
            generator->PrintLiteral("\"");
          // generator->PrintString(CEscape(value));
            
            if (isArrayEnd)
            {
                generator->PrintLiteral(line_sep_);
                if (!single_line_mode_) {
                  generator->Outdent();
                }
                generator->PrintLiteral("]");
            }
            
            if (i != last_field_idx) generator->PrintLiteral(",");
            
        }
        break;
      }
      case UnknownField::TYPE_GROUP:
        if (single_line_mode_) {
          generator->PrintLiteral("{ ");
        } else {
          generator->PrintLiteral("{\n");
          generator->Indent();
        }
        PrintUnknownFields(field.group(), generator);
        if (single_line_mode_) {
          generator->PrintLiteral("},");
        } else {
          generator->Outdent();
          generator->PrintLiteral("},");
        }
        
        break;
    }
      
      generator->PrintLiteral(line_sep_);
  }
    
    if (NULL != arrayIndexRange)
    {
        delete arrayIndexRange;
    }
    // STLDeleteElements(&arrayIndexes);
    while (!arrayIndexes.empty())
    {
        arrayIndexRange = arrayIndexes.front();
        delete arrayIndexRange;
        arrayIndexes.pop();
    }
}


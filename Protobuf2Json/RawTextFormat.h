#ifndef RAW_TEXT_FORMAT_H__
#define RAW_TEXT_FORMAT_H__


#include <string>
#include <vector>

#include <google/protobuf/stubs/common.h>
#include <google/protobuf/descriptor.h>
#include <google/protobuf/message.h>
#include <google/protobuf/message_lite.h>
#include <google/protobuf/io/coded_stream.h>
#include <google/protobuf/io/zero_copy_stream.h>
#include <google/protobuf/io/zero_copy_stream_impl.h>
#include <google/protobuf/descriptor.h>
#include <google/protobuf/unknown_field_set.h>
#include <google/protobuf/stubs/stl_util.h>


using namespace google::protobuf;
using namespace google::protobuf::io;
using namespace google::protobuf::compiler;


// This class implements protocol buffer text format.  Printing and parsing
// protocol messages in text format is useful for debugging and human editing
// of messages.
//
// This class is really a namespace that contains only static methods.
class RawTextFormat {
 public:


  class BaseTextGenerator {
   public:
    virtual ~BaseTextGenerator();

    virtual void Indent() {}
    virtual void Outdent() {}

    // Print text to the output stream.
    virtual void Print(const char* text, size_t size) = 0;

    void PrintString(const std::string& str) { Print(str.data(), str.size()); }

    template <size_t n>
    void PrintLiteral(const char (&text)[n]) {
      Print(text, n - 1);  // n includes the terminating zero character.
    }
  };
  
  // Class for those users which require more fine-grained control over how
  // a protobuffer message is printed out.
  class Printer {
   public:
    Printer();
    ~Printer();

    // Like TextFormat::Print
    bool Print(const Message& message, google::protobuf::io::ZeroCopyOutputStream* output) const;
    // Like TextFormat::PrintUnknownFields
    bool PrintUnknownFields(const UnknownFieldSet& unknown_fields,
                            google::protobuf::io::ZeroCopyOutputStream* output) const;
    // Like TextFormat::PrintToString
    bool PrintToString(const Message& message, std::string* output) const;
    // Like TextFormat::PrintUnknownFieldsToString
    bool PrintUnknownFieldsToString(const UnknownFieldSet& unknown_fields,
                                    std::string* output) const;
    // Like TextFormat::PrintFieldValueToString
    void PrintFieldValueToString(const Message& message,
                                 const FieldDescriptor* field, int index,
                                 std::string* output) const;

    // Adjust the initial indent level of all output.  Each indent level is
    // equal to two spaces.
    void SetInitialIndentLevel(int indent_level) {
      initial_indent_level_ = indent_level;
    }

    // If printing in single line mode, then the entire message will be output
    // on a single line with no line breaks.
    void SetSingleLineMode(bool single_line_mode) {
        single_line_mode_ = single_line_mode;
        line_sep_[0] = single_line_mode ? ' ' : '\n';
    }

    bool IsInSingleLineMode() const { return single_line_mode_; }

    // Set true to print repeated primitives in a format like:
    //   field_name: [1, 2, 3, 4]
    // instead of printing each value on its own line.  Short format applies
    // only to primitive values -- i.e. everything except strings and
    // sub-messages/groups.
    void SetUseShortRepeatedPrimitives(bool use_short_repeated_primitives) {
      use_short_repeated_primitives_ = use_short_repeated_primitives;
    }

    // If print_message_fields_in_index_order is true, fields of a proto message
    // will be printed using the order defined in source code instead of the
    // field number, extensions will be printed at the end of the message
    // and their relative order is determined by the extension number.
    // By default, use the field number order.
    void SetPrintMessageFieldsInIndexOrder(
        bool print_message_fields_in_index_order) {
      print_message_fields_in_index_order_ =
          print_message_fields_in_index_order;
    }

   private:
    // Forward declaration of an internal class used to print the text
    // output to the OutputStream (see text_format.cc for implementation).
      class TextGenerator;
      
      

    // Internal Print method, used for writing to the OutputStream via
    // the TextGenerator class.
    void Print(const Message& message, TextGenerator* generator) const;

    
    // Print the fields in an UnknownFieldSet.  They are printed by tag number
    // only.  Embedded messages are heuristically identified by attempting to
    // parse them.
    void PrintUnknownFields(const UnknownFieldSet& unknown_fields,
                            TextGenerator* generator) const;

    
    int initial_indent_level_;

    bool single_line_mode_;

    bool use_short_repeated_primitives_;

    bool print_message_fields_in_index_order_;

      
    char line_sep_[2];

  };


};


#endif  // RAW_TEXT_FORMAT_H__

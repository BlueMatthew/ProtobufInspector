# ProtobufInspector
A Fiddler extension for decoding Google Protobuf Data in requests and responses.  
  
## Files  
Protobuf2Json.dll  
Inspectors/ProtobufInspector.dll  
  
## Usage:  
Copy two released files into Fiddler installation folder.  

Once the extension is loaded by Fiddler, it will output a log as follows:  
![Loading Log](https://github.com/BlueMatthew/ProtobufInspector/raw/master/docs/res/LogOnLoading.png)  

## How to parse the protobuf data:  
1. If Content-Type in request/response headers is "application/x-protobuf", the extension will regard it as "Protobuf" data.  
2. The token value of "messageType" in "Content-Type" will be regarded as message type name.  
3. The token value of "desc" or "Desc" in "Content-Type" will be regarded as url of descriptorset of the protos.  
4. If message type or descriptorset url doesn't exist, the message type name and local proto folder can be provides in Protobuf Inspector View manually and then try to parse the data.  

## References:  
https://www.telerik.com/fiddler/add-ons  
https://github.com/protocolbuffers/protobuf  
https://github.com/maomaozgw/Protobuf2Fiddler  


//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: GetExeInfoMsg.proto
// Note: requires additional types generated from: ApplicationData.proto
// Note: requires additional types generated from: GameData.proto
namespace protocol
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GetExeInfoReq")]
  public partial class GetExeInfoReq : global::ProtoBuf.IExtensible
  {
    public GetExeInfoReq() {}
    
    private protocol.GetExeInfoReq.RequestType _requestType;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"requestType", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public protocol.GetExeInfoReq.RequestType requestType
    {
      get { return _requestType; }
      set { _requestType = value; }
    }
    [global::ProtoBuf.ProtoContract(Name=@"RequestType")]
    public enum RequestType
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"ALL", Value=0)]
      ALL = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"GAME", Value=1)]
      GAME = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"APPLICATION", Value=2)]
      APPLICATION = 2
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GetExeInfoRsp")]
  public partial class GetExeInfoRsp : global::ProtoBuf.IExtensible
  {
    public GetExeInfoRsp() {}
    
    private protocol.GetExeInfoRsp.ResultCode _resultCode;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"resultCode", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public protocol.GetExeInfoRsp.ResultCode resultCode
    {
      get { return _resultCode; }
      set { _resultCode = value; }
    }
    private readonly global::System.Collections.Generic.List<protocol.ApplicationItem> _applicationItem = new global::System.Collections.Generic.List<protocol.ApplicationItem>();
    [global::ProtoBuf.ProtoMember(2, Name=@"applicationItem", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<protocol.ApplicationItem> applicationItem
    {
      get { return _applicationItem; }
    }
  
    private readonly global::System.Collections.Generic.List<protocol.GameItem> _gameItem = new global::System.Collections.Generic.List<protocol.GameItem>();
    [global::ProtoBuf.ProtoMember(3, Name=@"gameItem", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<protocol.GameItem> gameItem
    {
      get { return _gameItem; }
    }
  
    [global::ProtoBuf.ProtoContract(Name=@"ResultCode")]
    public enum ResultCode
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"SUCCESS", Value=0)]
      SUCCESS = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"FAIL", Value=1)]
      FAIL = 1
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}
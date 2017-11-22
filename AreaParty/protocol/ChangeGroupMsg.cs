//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: ChangeGroupMsg.proto
// Note: requires additional types generated from: GroupData.proto
namespace protocol
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"ChangeGroupReq")]
  public partial class ChangeGroupReq : global::ProtoBuf.IExtensible
  {
    public ChangeGroupReq() {}
    
    private string _groupId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"groupId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string groupId
    {
      get { return _groupId; }
      set { _groupId = value; }
    }
    private protocol.ChangeGroupReq.ChangeType _changeType;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"changeType", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public protocol.ChangeGroupReq.ChangeType changeType
    {
      get { return _changeType; }
      set { _changeType = value; }
    }
    private readonly global::System.Collections.Generic.List<string> _userId = new global::System.Collections.Generic.List<string>();
    [global::ProtoBuf.ProtoMember(3, Name=@"userId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<string> userId
    {
      get { return _userId; }
    }
  

    private string _groupName = "";
    [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name=@"groupName", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string groupName
    {
      get { return _groupName; }
      set { _groupName = value; }
    }
    [global::ProtoBuf.ProtoContract(Name=@"ChangeType")]
    public enum ChangeType
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"ADD", Value=0)]
      ADD = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"DELETE", Value=1)]
      DELETE = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"UPDATE_INFO", Value=2)]
      UPDATE_INFO = 2
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"ChangeGroupRsp")]
  public partial class ChangeGroupRsp : global::ProtoBuf.IExtensible
  {
    public ChangeGroupRsp() {}
    
    private protocol.ChangeGroupRsp.ResultCode _resultCode;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"resultCode", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public protocol.ChangeGroupRsp.ResultCode resultCode
    {
      get { return _resultCode; }
      set { _resultCode = value; }
    }
    [global::ProtoBuf.ProtoContract(Name=@"ResultCode")]
    public enum ResultCode
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"SUCCESS", Value=0)]
      SUCCESS = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"FAIL", Value=1)]
      FAIL = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"NO_AUTHORITY", Value=2)]
      NO_AUTHORITY = 2
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"ChangeGroupSync")]
  public partial class ChangeGroupSync : global::ProtoBuf.IExtensible
  {
    public ChangeGroupSync() {}
    
    private protocol.GroupItem _groupItem;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"groupItem", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public protocol.GroupItem groupItem
    {
      get { return _groupItem; }
      set { _groupItem = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}
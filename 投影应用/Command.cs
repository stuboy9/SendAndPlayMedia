using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace myProjection
{

    [DataContract]
    class Command
    {
        [DataMember]
        public string name = "UWP";
        [DataMember]
        public string command = "";
        [DataMember]
        public Dictionary<string, string> param = new Dictionary<string, string>();
        /*
        public Command(string name,string command,Dictionary<string,string> param)
        {
            this.name = name;
            this.command = command;
            this.param = param;
        }*/
        public string ToJson()
        {
            JsonObject obj = new JsonObject();
            obj.Add("name", JsonValue.CreateStringValue(this.name));
            obj.Add("command", JsonValue.CreateStringValue(this.command));
            JsonObject dic = new JsonObject();
            foreach (KeyValuePair<string, string> pair in this.param)
            {
                dic.Add(pair.Key, JsonValue.CreateStringValue(pair.Value));
            }
            obj.Add("param", dic);
            return obj.ToString();
        }
        public static Command ToCommandFromJson(string json)
        {
            Command c = new Command();
            JsonObject obj = JsonObject.Parse(json);
            c.name = obj.GetNamedString("name");
            c.command = obj.GetNamedString("command");
            obj = obj.GetNamedObject("param");
            //obj.
            foreach (KeyValuePair<string, IJsonValue> pair in obj)
            {
                c.param.Add(pair.Key, pair.Value.GetString());
            }
            return c;
        }
    }
    [DataContract]
    class Response
    {
        [DataMember]
        public string status = "200";
        [DataMember]
        public string name = "UWP";
        [DataMember]
        public string type = "data";
        [DataMember]
        public List<string> value = new List<string>();

        public string ToJson()
        {
            JsonObject obj = new JsonObject();
            obj.Add("name", JsonValue.CreateStringValue(this.name));
            obj.Add("status", JsonValue.CreateStringValue(this.status));
            obj.Add("type", JsonValue.CreateStringValue(this.type));
            JsonArray lis = new JsonArray();
            foreach (string item in this.value)
            {
                lis.Add(JsonValue.CreateStringValue(item));
            }
            obj.Add("value", lis);
            return obj.ToString();
        }
        public static Command ToCommandFromJson(string json)
        {

            return null;
        }
    }
}

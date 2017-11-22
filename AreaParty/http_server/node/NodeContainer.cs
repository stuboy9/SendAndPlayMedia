using System.Collections.Generic;

namespace node
{

	public class NodeContainer
	{
		private static Dictionary<string, Node> nodes = new Dictionary<string, Node>();
		public static Node getNode(string id)
		{
			return nodes[id];
		}
		public static bool hasNode(string id)
		{
			return nodes.ContainsKey(id);
		}
		public static void addNode(Node node)
		{
			if (nodes.Count >= 10000)
			{
				deletAll(); 
			}
			if (!nodes.ContainsKey(node.id))
			{
				nodes[node.id] = node;
			}
		}
		public static void deletNode(string id)
		{
			nodes.Remove(id);
		}
		public static void deletAll()
		{
			nodes.Clear();
		}

	}

}
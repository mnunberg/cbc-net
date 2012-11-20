using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Couchbase.Operations;
using Enyim.Caching.Memcached;
using Enyim.Caching.Memcached.Results;
using Couchbase.Results;

namespace Couchbase.Cbc.Commands
{
	class CbcSet : CommandBase
	{	
		public string Value;

		public CbcSet(CouchbaseClient cli,
			string key, CbcOptions options)
			: base(cli, key, options)
		{
			Value = options.Value;
			if (Value == null)
			{
				throw new ArgumentException("Set command needs value");
			}
		}

		private Dictionary<string,StoreMode> modeMap = new Dictionary<string,StoreMode>()
		{
			{ "add", StoreMode.Add },
			{ "replace", StoreMode.Replace },
			{ "set", StoreMode.Set }
		};

		public override bool Execute()
		{
			ICasOperationResult res;
			UTF8Encoding utf8 = new UTF8Encoding();
			byte[] vbytes = utf8.GetBytes(Value);
			if (modeMap.ContainsKey(options.Command))
			{
				StoreMode mode = modeMap[options.Command];
				Console.Error.WriteLine("Arguments to ExecuteStore: ");
				Console.Error.WriteLine(
					String.Format(
					"0x{0:x}, '{1}', '{2}', {3}, {4}, {5}",
					mode, Key, Value, options.Expiry, options.Persist, options.Replicate));

				res = client.ExecuteStore(
					mode, Key, Value, options.Expiry, options.Persist, options.Replicate);
				if (!res.Success)
				{
					Console.Error.WriteLine("Initial command failed. Trying simple version..");
					Console.Error.WriteLine(res.Message);
					if (res.InnerResult != null)
					{
						Console.Error.WriteLine(res.InnerResult.Message);
					}
					res = client.ExecuteStore(mode, Key, Value);
				}
			}
			else if (options.Command == "append")
			{
				res = client.ExecuteAppend(Key,
					options.Cas,
					new ArraySegment<byte>(vbytes));
			}
			else if (options.Command == "prepend")
			{
				res = client.ExecutePrepend(Key,
					options.Cas,
					new ArraySegment<byte>(vbytes));
			}
			else
			{
				throw new ArgumentException("Unknown command");
			}

			if (!res.Success)
			{
				FailCommand(res);
				return false;
			}

			Console.WriteLine(
				"Operation Succeeded. Cas: " +
				res.Cas.ToString());


			if (options.Persist != PersistTo.Zero ||
				options.Replicate != ReplicateTo.Zero)
			{
				System.Console.WriteLine("Trying observe now...");
				new CbcObserve(client, Key, options, res.Cas).Execute();
			}

			return true;
		}
	}
}
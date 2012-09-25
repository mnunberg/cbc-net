using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Couchbase.Results;
using Couchbase.Operations;

namespace Couchbase.Cbc.Commands
{
	class CbcObserve : CommandBase
	{
		public UInt64 Cas;

		public CbcObserve(CouchbaseClient cli,
			string key, CbcOptions options, UInt64 cas = 0) :
			base(cli, key, options)
		{
			if (cas > 0)
			{
				Cas = cas;
			}
			else if (options.Cas > 0 )
			{
				Cas = options.Cas;
			}
			else
			{
				throw new ArgumentException("Invalid CAS");
			}
		}

		public override bool Execute()
		{
			IObserveOperationResult res = client.Observe(
				Key, Cas, options.Persist, options.Replicate);

			if (!res.Success)
			{
				FailCommand(res);
				return false;
			}
			
			string pretty = String.Format("Observe Code: (0x{0:x})",
				(int)res.KeyState);

			foreach (string name in Enum.GetNames(typeof(ObserveKeyState)))
			{
				ObserveKeyState v = (ObserveKeyState)
					Enum.Parse(typeof(ObserveKeyState), name);
				if (res.KeyState == v)
				{
					pretty += " " + name;
					break;
				}
			}

			Console.WriteLine(pretty);

			pretty = String.Format(
				"Timings: Persist [{0}], Replicate [{1}]",
				res.PersistenceStats.ToString(),
				res.ReplicationStats.ToString());
			Console.WriteLine(pretty);

			return true;
		}
	}

	class CbcObserveWrap : CbcObserve
	{
		public CbcObserveWrap(CouchbaseClient cli, string key, CbcOptions opts)
			:base(cli, key, opts)
		{
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enyim.Caching.Memcached.Results;

namespace Couchbase.Cbc.Commands
{
	class CbcGet : CommandBase
	{
		public CbcGet(CouchbaseClient cli,
			string key,
			CbcOptions opts)
			: base(cli, key, opts) { }

		public override bool Execute()
		{
			IGetOperationResult<string> gres 
				= client.ExecuteGet<string>(Key);
			if (!gres.Success)
			{
				FailCommand(gres);
				return false;
			}
			Console.Error.WriteLine("Command OK");
			Console.Error.WriteLine("Value is " + gres.Value);
			Console.Error.WriteLine("Cas is " + gres.Cas.ToString());
			return true;
		}
	}
}

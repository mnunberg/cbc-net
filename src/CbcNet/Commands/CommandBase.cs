using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enyim.Caching.Memcached.Results;

namespace Couchbase.Cbc.Commands
{
	interface ICommand
	{
		bool Execute();
	}

	abstract class CommandBase : ICommand
	{
		public string Key;
		protected CouchbaseClient client;
		protected CbcOptions options;

		public CommandBase(CouchbaseClient client,
			string key,
			CbcOptions options)
		{
			Key = key;
			this.client = client;
			this.options = options;
		}

		protected void FailCommand(IOperationResult res)
		{
			Console.Error.WriteLine("Command Failed");
			Console.Error.WriteLine("Code is " + res.StatusCode.ToString());
			Console.Error.WriteLine("Message is " + res.Message);
		}

		public abstract bool Execute();
	}
}

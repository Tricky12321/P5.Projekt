using System;
namespace Serial.DataMapper
{
	public class TooManyDataEntriesRequestedException : Exception
	{
		public TooManyDataEntriesRequestedException(string message) : base(message)
		{
			
		}

		public TooManyDataEntriesRequestedException(string message, Exception inner) : base(message, inner)
		{
			
		}
	}
}

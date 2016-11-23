using System;
using Microsoft.Extensions.Internal;

namespace kwd_keepass.tests.TestHelpers
{
    public class StubClock : ISystemClock
    {
        public StubClock() { }
        public StubClock(DateTime when) { UtcNow = new DateTimeOffset(when);}

        public DateTimeOffset UtcNow { get; set; }
    }
}

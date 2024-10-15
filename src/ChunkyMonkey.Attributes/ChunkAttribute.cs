using System;

namespace ChunkyMonkey.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ChunkAttribute(MemberAccessor memberAccessor = MemberAccessor.Public) : Attribute
    {
        public MemberAccessor MemberAccessor { get; } = memberAccessor;
    }
}

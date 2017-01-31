namespace EasyHash.Specs.Targets
{
    using System;
    using System.Collections.Generic;

    public class Foo : ICloneable
    {
        public int Number { get; set; }

        public string Text { get; set; }

        public IEnumerable<string> Words { get; set; }

        public int[] Years { get; set; }

        public Foo[] Foos { get; set; }

        public object Clone() => new Foo() { Number = Number, Text = Text, Years = Years, Words = Words };
        public override int GetHashCode() => EasyHash<Foo>.GetHashCode(this);
        public override bool Equals(object obj) => EasyHash<Foo>.Equals(this, obj);
    }
}

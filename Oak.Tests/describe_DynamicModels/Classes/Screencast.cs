using System;
using System.Collections.Generic;
using System.Linq;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Screencast : DynamicModel
    {
        Presenters presenters = new Presenters();

        Screencasts screencasts = new Screencasts();

        Tags tags = new Tags();

        public Screencast(object dto) : base(dto) { }

        public Screencast() { }

        IEnumerable<dynamic> Associates()
        {
            yield return new HasManyAndBelongsTo(presenters, screencasts);

            yield return new HasManyAndBelongsTo(tags, screencasts);
        }
    }
}

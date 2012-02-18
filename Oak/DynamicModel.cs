using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;

namespace Oak
{
    public class DynamicModel : Gemini
    {
        public DynamicModel(object dto)
            : base(dto)
        {

        }

        public DynamicModel()
            : this(new { })
        {

        }

        void Initialize()
        {
            new MixInValidation(this);

            new MixInAssociation(this);

            var dtoMembers = this.ToDictionary().ToList();

            foreach (var item in dtoMembers) SetMember(item.Key, item.Value);

            new MixInChanges(this);
        }
    }
}

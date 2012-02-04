using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Association
{
    class describe_MixInAssociations : nspec
    {
        dynamic gemini;

        void given_plain_old_gemini()
        {
            xcontext["mixed in with associations"] = () =>
            {
                before = () =>
                {
                    gemini = new Gemini(new
                    {
                        Associates = new DynamicEnumerableFunction(() => 
                        {
                            return new dynamic[] 
                            {
                                new HasMany(new Blogs())
                            };
                        })
                    });

                    new MixInAssociation2(gemini);
                };

                it["responds to association"] = () =>
                    ((bool)gemini.RespondsTo("Blogs")).should_be_true();
            };
        }
    }
}

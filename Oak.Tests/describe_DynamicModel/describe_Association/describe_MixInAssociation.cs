using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Association
{
    public class describe_MixInAssociation : nspec
    {
        dynamic plainOldGemini;

        void given_gemini_with_assocation_definition()
        {
            before = () =>
            {
                plainOldGemini = new Gemini(new 
                { 
                    Associates = new DynamicEnumerableFunction(() => 
                    {
                        List<dynamic> associates = new List<dynamic>();

                        associates.Add(new HasMany(new Blogs()));

                        return associates;
                    })
                });
            };

            context["gemini has associates mixed in"] = () =>
            {
                before = () => new MixInAssociationV2(plainOldGemini);

                it["gemini responds to methods defined in assocation"] = () => 
                {
                    ((bool)plainOldGemini.RespondsTo("Blogs")).should_be_true();
                };
            };
        }
    }
}

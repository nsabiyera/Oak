using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Validation.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Validation
{
    class uniquness_for_static_class : uniqueness
    {
        void before_each()
        {
            users.Projection = d => new UserWithAutoProperties(d);
        }

        public override dynamic NewUser()
        {
            return new UserWithAutoProperties();
        }
    }
    
    class uniquness_for_dynamic_class : uniqueness
    {
        void before_each()
        {
            users.Projection = d => new User(d);
        }

        public override dynamic NewUser()
        {
            return new User();
        }
    }

    class uniquness_for_dynamic_class_with_deferred_error_message : uniqueness
    {
        void before_each()
        {
            users.Projection = d => new UserWithDeferredError(d);
        }

        public override dynamic NewUser()
        {
            return new UserWithDeferredError();
        }
    }
    
    abstract class uniqueness : nspec
    {
        public Seed seed;

        public Users users;

        public dynamic user;

        public dynamic userId;

        void before_each()
        {
            seed = new Seed();

            users = new Users();
        }

        public abstract dynamic NewUser();

        void describe_uniqueness()
        {
            before = () =>
            {
                seed.PurgeDb();

                seed.CreateTable("Users", new dynamic[] {
                    new { Id = "int", Identity = true, PrimaryKey = true },
                    new { Email = "nvarchar(255)" }
                }).ExecuteNonQuery();

                userId = new { Email = "user@example.com" }.InsertInto("Users");
            };

            context["email associated with users is not taken"] = () =>
            {
                before = () =>
                {
                    user = NewUser();
                    user.Email = "nottaken@example.com";
                };

                it["user is valid"] = () => ((bool)user.IsValid()).should_be_true();
            };

            context["email associated with users is taken"] = () =>
            {
                before = () =>
                {
                    user = NewUser();
                    user.Email = "user@example.com";
                };

                it["user is invalid"] = () => ((bool)user.IsValid()).should_be_false();

                it["error message states the user is taken"] = () =>
                {
                    user.IsValid();

                    (user.FirstError() as string).should_be("User user@example.com is taken.");
                };
            };

            context["email that is taken belongs to current user"] = () =>
            {
                before = () => user = users.Single(userId);

                it["users is valid because the taken email belongs to current user"] = () => ((bool)user.IsValid()).should_be_true();
            };
        }
    }
}

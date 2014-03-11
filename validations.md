##Links
- [General Usage](https://github.com/amirrajan/Oak/wiki/Adding-validation-using-Oak.DynamicModel#general-usage) 
- [Acceptance](https://github.com/amirrajan/Oak/wiki/Adding-validation-using-Oak.DynamicModel#acceptance) - A legal document's terms must be accepted.
- [Confirmation](https://github.com/amirrajan/Oak/wiki/Adding-validation-using-Oak.DynamicModel#confirmation) - A password must be confirmed.
- [Exclusion](https://github.com/amirrajan/Oak/wiki/Adding-validation-using-Oak.DynamicModel#exclusion) - A username cannot be "admin" or "administrator".
- [Format](https://github.com/amirrajan/Oak/wiki/Adding-validation-using-Oak.DynamicModel#format) - A product code must match this regex.
- [Inclusion](https://github.com/amirrajan/Oak/wiki/Adding-validation-using-Oak.DynamicModel#inclusion) - A coffee comes in three sizes only: "small", "medium" and "large".
- [Numericality](https://github.com/amirrajan/Oak/wiki/Adding-validation-using-Oak.DynamicModel#numericality) - A number can be only integers, greater than 10.
- [Presence](https://github.com/amirrajan/Oak/wiki/Adding-validation-using-Oak.DynamicModel#presence) - A book's title is required.
- [Uniqueness](https://github.com/amirrajan/Oak/wiki/Adding-validation-using-Oak.DynamicModel#uniqueness) - A username must be unique in the database.
- [Conditional Validation](https://github.com/amirrajan/Oak/wiki/Adding-validation-using-Oak.DynamicModel#conditional-validation) - Conditions applied to validations.

##General Usage
DynamicModel allows you to extend behavior of a class dynamically.  This mechanism is used by Cambium's Validations Module.  A DynamicModel MUST be defined as follows:

    public class Blog : DynamicModel
    {
        public Blog() : this(new { }) { } //parameterless contructor is optional

        public Blog(object dto) : base(dto) { }
    }

Once you have defined the basic DynamicModel, you can add validation by adding the following method:
    
    public class Blog : DynamicModel
    {
        public Blog() : this(new { }) { } //parameterless contructor is optional

        public Blog(object dto) : base(dto) { }

        //adding this method to your DynamicModel gives it validation capabilities
        public IEnumerable<dynamic> Validates()
        {
            yield return //some form of validation.
        }
    }


By adding the `public IEnumerable<dynamic> Validates()` method to your DynamicModel will get you access to the following api:
    
    dynamic blog = new Blog(new { Title = "Some Title" }); //DynamicModel must be assigned to dynamic
    blog.IsValid();  //this method will execute validation and either return true or false
    blog.FirstError();  //this method will return a string with the first error
    blog.Errors();  //returns a List<KeyValuePair<string, string>> of all errors
    blog.IsPropertyValid("Title"); //returns true of false for a single property

###Validation Options
Here are the validation options that are currently available along with sample usage.

####Acceptance
<a name="acceptance" id="acceptance" href="#acceptance"></a> 

    dynamic legalDocument = new LegalDocument();
    legalDocument = new LegalDocument();
    legalDocument.TermsOfService = true;
    legalDocument.TypedOutAcceptance = "I Agree";
    legalDocument.IsValid(); //this will evaluate to true

    public class LegalDocument : DynamicModel
    {
        public LegalDocument()
        {
           
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Acceptance("TermsOfService");

            yield return new Acceptance("TypedOutAcceptance")
            {
                Accept = "I Agree",
                ErrorMessage = "You have not typed out the acceptance. Type I Accept."
                /*
                The ErrorMessage property can be defined for lazy evaluation using the following:
                ErrorMessage = 
                    new DynamicFunction(() => 
                        "You have not typed out the acceptance. Type I Accept. The current value is: " + _.TypedOutAcceptance);
                */
            };
        }
    }

####Confirmation

    dynamic person = new Person();
    person.Email = "user@example.com";
    person.EmailConfirmation = "user@example.com";
    person.IsValid(); //this will evaluate to true

    public class Person : DynamicModel
    {
        public Person() : this(new { })
        {
        }

        public Person(object dto)
        {
            
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Confirmation("Email"); //an EmailConfirmation property will get added for you
        }
    }
  

####Exclusion

    dynamic registration = new Registration();
    registration.UserName = "admin";
    registration.IsValid(); //this will be false

    public class Registration : DynamicModel
    {
        public Registration()
        {
            
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Exclusion("UserName") { In = new[] { "admin", "administrator" } };
        }
    }

####Format

    dynamic product = new Product();
    product.Code = "ABD";
    product.IsValid(); //this will evaluate to true
    product.Code = "1121212";
    product.IsValid(); //this will evaluate to false
    
    public class Product : DynamicModel
    {
        public Product()
        {
            
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Format("Code") { With = @"\A[a-zA-z]+\z" };
        }
    }

####Inclusion

    dynamic coffee = new Coffee();
    coffee.Size = "small";
    coffee.IsValid(); //this will evaluate to true
    coffee.Size = "tall";
    coffee.IsValid(); //this will evaluate to false
    
    public class Coffee : DynamicModel
    {
        public Coffee()
        {
            
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Inclusion("Size") { In = new[] { "small", "medium", "large" } };
        }
    }

####Numericality

    public class Player : DynamicModel
    {
        public Player() : this(new { })
        {
        }

        public Player(object dto)
        {
            
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Numericality("AveragePoints");
            yield return new Numericality("Age") { OnlyInteger = true };
            yield return new Numericality("HeightInInches") { GreaterThan = 60, LessThan = 72 };
            yield return new Numericality("WeightInPounds") { GreaterThanOrEqualTo = 185, LessThanOrEqualTo = 300 };
            yield return new Numericality("NumberOfFingers") { EqualTo = 10 };
            yield return new Numericality("LuckyEvenNumber") { Even = true };
            yield return new Numericality("LuckyOddNumber") { Odd = true };
        }
    }

####Presence

    dynamic book = new Book();
    book.IsValid(); //book will be invalid
    book.Title = "Some Title";
    book.Body = "Some Body";
    book.IsValid(); //book will now be valid

    public class Book : DynamicModel
    {
        public Book() : this(new { })
        {

        }

        public Book(object dto)
        {
            
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Presence("Title");
            yield return new Presence("Body");
        }
    }

####Uniqueness

    //given    
    Seed seed = new Seed();

    seed.CreateTable("Users", new dynamic[] {
        new { Id = "int", Identity = true, PrimaryKey = true },
        new { Email = "nvarchar(255)" }
    }).ExecuteNonQuery();

    var userId = new { Email = "user@example.com" }.InsertInto("Users");

    dynamic user = new User();
    user.Email = "nottaken@example.com";
    user.IsValid(); //this should be true
    user.Email = "user@example.com";
    user.IsValid(); //this should be false
    
    public class Users : DynamicRepository
    {
        public Users()
        {
            Projection = d => new User(d);
        }
    }

    public class User : DynamicModel
    {
        Users users;

        public User() : this(new { })
        {
        }

        public User(object dto)
        {
            users = new Users();
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Uniqueness("Email", users);
        }
    }

####Conditional Validation

Validation can be executed conditionally:

    public class Order : DynamicModel
    {
        public Order()
        {
            
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Presence("CardNumber") { If = d => d.PaidWithCard() };

            yield return new Presence("Address") { Unless = d => d.IsDigitalPurchase() };
        }

        public bool PaidWithCard()
        {
            return This().PaymentType == "Card";
        }

        public bool IsDigitalPurchase()
        {
            return This().ItemType == "Digital";
        }
    }

####Creating Your Own Validators

To create your validation, create a class that inherits from `Oak.Validation` and define a `public bool Validate(dynamic entity)` method. The `Validate` method will be given a reference to the entire dynamic object when `IsValid()` is called.

Here is an example of how you can add validation for an empty `Guid`.

    public class GuidNotEmpty : Validation
    {
      public GuidNotEmpty(string property) : base(property) { }

      public override void Init(dynamic entity)
      {
        base.Init(entity as object);

        if (string.IsNullOrEmpty(ErrorMessage)) ErrorMessage = Property + " is required.";
      }

      public bool Validate(dynamic entity)
      {
        var value = PropertyValueIn(entity);

        return value != Guid.Empty;
      }
    }

For other examples of validation, take a look at the [Validations module's source code](https://github.com/amirrajan/Oak/blob/master/Oak/Validation.cs#L175)

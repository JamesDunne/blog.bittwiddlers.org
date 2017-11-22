---
author: jaymz
categories:
- Uncategorized
date: '2011-02-14T00:33:31'
tags: []
title: Immutable DataContract generator for Visual Studio 2008 T4
---
At a first glance, using WCF appears to limit one's capabilities in working
with immutable data structures, namely immutable data contracts (classes
decorated with [DataContract] and [DataMember]) attributes. After some thought
and a little experimentation, I came to a reasonable solution implemented in
T4 where one can code-generate the immutable data structure and its related
mutable class builder structure used to construct said immutable data contract
instances. First, let me demonstrate and explain a bit of the basic code
pattern behind immutable data contracts before we move onto the T4 solution.
Hopefully, the code generation opportunities may strike you as obvious as they
did to me.

    
    
    [DataContract(Name = "DataContract1")]
    public class DataContract1
    {
        [DataMember(Name = "ID")]
        private int _ID;
        [DataMember(Name = "Code")]
        private string _Code;
        [DataMember(Name = "Name")]
        private string _Name;
    
        private DataContract1() { }
    
        public int ID { get { return this._ID; } }
        public string Code { get { return this._Code; } }
        public string Name { get { return this._Name; } }
    
        public sealed class Mutable
        {
            public int ID { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
    
        public static implicit operator DataContract1(Mutable mutable)
        {
            var imm = new DataContract1();
            imm._ID = mutable.ID;
            imm._Code = mutable.Code;
            imm._Name = mutable.Name;
            return imm;
        }
    }
    

This is the very basic, bare-bones code for a fully-functional immutable data
contract. WCF, requiring full-trust access, will serialize and deserialize
to/from the private fields marked with the [DataMember] attributes at runtime
using reflection in its DataContractSerializer. Note that public instantiation
of the immutable DataContract1 class is strictly forbidden as we only have a
default private constructor. No one may access our private fields either. The
only exposed public properties are read-only. Now let's look at the nested
Mutable class. This is what you would normally expect to see a regular mutable
DataContract implemented as, except it has no [DataContract] attribute on its
class declaration nor any [DataMember] attributes decorated on its public
properties, so we cannot use it as a WCF data contract (well, strictly
speaking, we could, but it wouldn't be recommended as it has virtually no
metadata exposed for it). The real interesting part is this implicit
conversion operator defined in the main immutable DataContract1 class. This is
a nary-used, albeit powerful feature of C#. One may implement an implicit or
explicit conversion operator to allow two classes or structs to be convertible
between one another (or only in one direction) using either an implicit cast-
like behavior or an explicit one at the "call site". A conversion operator
operates much like a traditional type cast. You've seen these everywhere. It's
when you do `object a; string b = (string)a;` That (string)a expression is a
unary (takes one operand) cast expression. It tells the compiler that you, as
the developer, knows for absolute sure that the object stored in `a` is of
type `string` and to store the string reference value in the `b` variable. An
explicit conversion operator is closest in appearance to the cast expression.
The difference between implicit and explicit is that implicit does not require
you to write a cast expression at the conversion site where the conversion is
required. In our case with the immutable (DataContract1) and mutable
(DataContract1.Mutable) classes, our implicit conversion operator allows us to
"convert" the mutable builder class into a new instance of the immutable
class. This will appear completely transparent to the developer, which is a
nice feature. A usage example of this pattern would be as follows:

    
    
    class Program
    {
        static DataContract1 CreateDC1()
        {
            return new DataContract1.Mutable()
            {
                ID = 27,
                Code = "JSD",
                Name = "James S. Dunne"
            };
            // Notice that the `DataContract1.Mutable` reference in the return statement is implicitly converted to
            // the required `DataContract1` return type via the static implicit operator we defined.
        }
    
        static void Main(string[] args)
        {
            var dc1 = CreateDC1();
            WriteDC1(dc1);
        }
    
        static void WriteDC1(DataContract1 dc1)
        {
            Console.WriteLine(dc1.ID.ToString());
            Console.WriteLine(dc1.Code);
            Console.WriteLine(dc1.Name);
    
            dc1.Name = "Bob Smith";              // ERROR! Cannot modify because the property is read-only.
        }
    }
    

This small program is demonstrating how one would use the Mutable nested
builder class to construct an immutable instance from. Creating immutable
instances of objects should be done once and only once. From there it is
impossible to modify (mutate) them further, unless the types of your
properties exposed are mutable themselves. This immutability guarantee is
therefore shallow, and it is up to you to guarantee deep immutability through
all of your exposed types. For instance, if you expose a List<T> property via
this immutable structure, other functions are free to modify your List by
adding, removing, and modifying elements within it, but they cannot change the
property to point to a wholly different List<T> reference. For this reason, it
is recommended to make use of the .NET framework's read-only immutable
collection classes for exposing lists within your data contract. As a matter
of self-discipline, I avoid embedding List<T> and other collection types
within my data contracts. I also keep my data contracts free of object
references, thereby reducing the potential object graph to a degenerate case
of a single object. **T4 Solution** Now that we see the basic immutable data
contract pattern and how to use it, let's explore opportunities to create a T4
code generation solution to make our lives easier so that we don't have to
type up all this boilerplate for our multitudes of data contracts that we have
to expose over our service layer and use in our business logic.

    
    
    [Immutable, DataContract(Name = "DataContract1")]
    public partial class DataContract1
    {
        /// <summary>
        /// The unique identifier of the data contract.
        /// </summary>
        [DataMember(Name = "ID")]
        private int _ID;
    
        [DataMember(Name = "Code")]
        private string _Code;
    
        [DataMember(Name = "Name")]
        private string _Name;
    }
    

This is all the metadata and implementation code that the developer should be
required to write in order to help the T4 generate the rest of the
boilerplate. The T4 template will generate an output C# source file looking
something like this:

    
    
    public partial class DataContract1
    {
        #region Constructors
    
        private DataContract1()
        {
        }
    
        public DataContract1(
            int ID,
            string Code,
            string Name
        )
        {
            this._ID = ID;
            this._Code = Code;
            this._Name = Name;
        }
    
        #endregion
    
        #region Public read-only properties
    
        /// <summary>
        /// The unique identifier of the data contract.
        /// </summary>
        public int ID
        {
            get { return this._ID; }
        }
    
        public string Code
        {
            get { return this._Code; }
        }
    
        public string Name
        {
            get { return this._Name; }
        }
    
        #endregion
    
        #region Mutable nested builder class
    
        /// <summary>
        /// A mutable class used to assign values to be ultimately copied into an immutable instance of the parent class.
        /// </summary>
        public sealed class Mutable : ICloneable
        {
            #region Public read-write properties
    
            /// <summary>
            /// The unique identifier of the data contract.
            /// </summary>
            public int ID { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
    
            #endregion
    
            #region ICloneable Members
    
            /// <summary>
            /// Clones this Mutable instance to a new Mutable instance using simple property assignment.
            /// </summary>
            public object Clone()
            {
                return new Mutable()
                {
                    ID = this.ID,
                    Code = this.Code,
                    Name = this.Name,
                };
            }
    
            #endregion
        }
    
        #endregion
    
        #region Conversion operators
    
        /// <summary>
        /// Copies the values from the Mutable instance into a new immutable instance of this class.
        /// </summary>
        public static implicit operator DataContract1(Mutable mutable)
        {
            var imm = new DataContract1();
            imm._ID = mutable.ID;
            imm._Code = mutable.Code;
            imm._Name = mutable.Name;
            return imm;
        }
    
        /// <summary>
        /// Copies the values from this immutable instance into a new Mutable instance.
        /// </summary>
        public static explicit operator Mutable(DataContract1 immutable)
        {
            var mutable = new Mutable();
            mutable.ID = immutable._ID;
            mutable.Code = immutable._Code;
            mutable.Name = immutable._Name;
            return mutable;
        }
    
        #endregion
    }
    

Notice that this is a lot more verbose that the abridged version posted for
demonstration purposes at the top of this blog. How does this all work? The T4
template makes use of EnvDTE, the namespace that Visual Studio exposes for
doing in-editor code refactoring work, intended primarily for use by Visual
Studio add-ins. The project does not have to be compiled in order for this to
work. I decided against using the Microsoft.Cci or regular reflection APIs
since these require a compiled assembly in order to do their work. Since this
pattern has to be implemented in a single type in a single assembly, neither
of those are acceptable solutions. The alternative is to have the developer
design their data contracts in some other format that could be parsed by the
T4 template and have it generate all the code on behalf of the developer.
Since this solution would raise the learning curve already high enough for
immutable data structures, this is thrown out as well. Having the developer
code up part of the immutable data contract him/herself is beneficial in that
the developer has some control over the class itself and is able to add
methods, fields, and properties (with some restrictions) yet also gain the
benefits of code generation provided by the T4 template. Of course, to
maintain true immutability, it doesn't make much sense to have any publicly
writable properties. The T4 template imposes some limitations like this on the
existing partial class code. The T4 template also heavily relies on naming
conventions so those are checked and enforced before generating a single line
of code. T4 will generate descriptive and helpful warnings in your Error List
pane if it detects anything is awry with the existing partial class code.
**Download the code** My code for the T4 template is housed within the
following public SVN repository: Get it
[here](tsvn:svn://bittwiddlers.org/WellDunne/trunk/public/ImmutableT4)
(svn://bittwiddlers.org/WellDunne/trunk/public/ImmutableT4). Or browse the
repository over the web from
[here](http://bittwiddlers.org/viewsvn/trunk/public/ImmutableT4/?root=WellDunne).
The file is named
[ImmutablePartialsGenerator.tt](tsvn:svn://bittwiddlers.org/WellDunne/trunk/public/ImmutableT4/ImmutableT4/ImmutablePartialsGenerator.tt)
and lives in the project folder named ImmutableT4 under the main solution
folder. Browse the latest version
[here](http://bittwiddlers.org/viewsvn/trunk/public/ImmutableT4/ImmutableT4/ImmutablePartialsGenerator.tt?view=markup&root=WellDunne).
**Note** that the T4 template requires Visual Studio's EnvDTE namespace to
have a complete view of the code, so if it does not work within the first
moments of opening the project, do not be surprised. Give it time to parse
over your code and build a complete model and then try the T4 template again.
**This is a limitation of Visual Studio and not of this template**.
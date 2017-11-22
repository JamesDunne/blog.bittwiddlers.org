---
author: jaymz
categories:
- Uncategorized
date: '2013-05-22T03:10:54'
tags: []
title: Thoughts on Go 1.1
---
I'd like to share a few thoughts I have about the Go programming language
after implementing my very first and currently only project in it. This may be
a bit premature since I don't have much experience with it, so if you have
some advice to give or some justifications to make then please comment back.
I'm always eager to learn new things!

For future readers, it should be known that at the time of this writing
(2013-05-22), Go 1.1 was just recently released, so all of this observation is
specific to that version and not to any newer version that obviously doesn't
exist yet.

Fair warning: there are some strong opinions expressed here. I make no apology
for having strong opinions, but perhaps the tone in which those opinions are
expressed might be offensive and I will preemptively apologize for that. It's
hard for me to decouple the passion from the tone.

**Language features:**

First off, let's address the biggest elephants in the room:

  1. usage of `nil` instead of the much more common `null` to represent the lack of a value for a reference type
  2. **non-nullable strings**
  3. `import` with an unused package is a **compiler error**
  4. identifier case determines package exposure

I don't think that `nil` and `null` in terms of reference values (or the
absence of such) are two different concepts here so there's really no reason
that I can see for going with `nil` over `null`. It seems contrarian in
nature. I'll just dispense with the `nil`lity and say `null` from now on and
you should know what I mean.

Strings in the Go language act like reference types, and since all other
reference types are nullable, why not strings? The idea that the empty string
is equivalent to the null string is utter nonsense. Anyone who preaches or
practices this has no appreciation for the real expressive value of
nullability or optionality. Having a way to represent a missing value as
opposed to an empty value (or zero value) is a good thing.

Now, if strings were non-nullable AND there were a more general optionality
feature of the type system to make any non-nullable type into a nullable one,
THEN that would be nice. In that case, the nullability of a type would be
decoupled from the type itself and I would agree then that `string` should be
non-nullable, like every other basic type should be. I've yet to see this kind
of clean type system design in the family of curly-brace languages. An example
syntax off the top of my head would be `string?` (nullable string) vs.
`string` (non-nullable default string) and `int?` vs. `int` and `bool?` vs.
`bool`, etc. You see where I'm going.

The most popular complaint that I've seen is that all imported packages must
be used or you get a compiler error. This compiler error is just downright
stupid. I see the intention, and I can kinda get why this was done. But the
developers chose to stick to their guns and suggest workarounds for the
obvious deficiency, and this is where things get worse. The suggested
workaround is to _define a dummy variable in your code using some exported
member of the package_. This workaround is a _worse_ code smell than the
original problem of having an "unclean" import section! What were they
thinking?! Nonsense. Give me a compiler option to turn that stupidity off at
the very least. I should be the one who decides whether an import list should
be exact or not, not my compiler nor its over-zealous authors. We'll revisit
this a little bit later in a dumb little narrative.

Riding on the package import error's heels is the requirement that public
members of packages must start with an uppercase character. Character case
should not decide such an important aspect and also somewhat volatile fact of
a package's member. During development you might start out with everything
private and then maybe wish to expose things later, or even vice versa. Having
to change the exposure of a package member will mean having to rename all
instances of its usage. What a needless pain. It also makes the export list of
the package less discoverable. An `export` clause at the top of the package
file would do fine and serve as better documentation.

There are other issues with forced character casing that arise in marshalling
of data to JSON and XML, for instance. Granted there are "tags" that one can
apply to struct members in order to provide marshalling hints but the simple
fact that you can't cleanly represent your struct members as close to how you
wish to represent the marshalled data is a shame.

Now that the big elephants are out of the way, the rest of the language is
more or less competent. The only other major complaint at this point would be
the lack of generics. You can't really _cleanly_ bolt generics onto an
already-released language. C# and Java both learned that lesson the hard way.
It really has to be baked in from the start. That is, of course, unless you
want to just cut a swath of breaking changes in with version 2.0 of your
language to get generics in. I guess it depends on the boldness of the
language development team. I personally would be fine with breaking changes if
they introduced a much more powerful feature that took out a lot of warts and
inconsistencies.

There is a bit of silliness that arises from the consequences of how
semicolons are elided at the lexer level. For instance, if you separate out a
method call expression onto multiple lines where each line is a parameter
expression terminated by a comma, then the last parameter line must also
terminate with a comma even if the very last line contains the closing paren
of the method call expression. Perhaps an example will help:

    
    
    method(
        param1,
        param2,  // <- this comma is **required**
    )
    

This isn't a huge deal, but it does sorta make things look messy. Now, I'm all
for acceptable usage of extra trailing commas in things like list initializers
because they're useful there, but for a standard method call expression that
doesn't have a variable number of parameters it's kind of misleading. Your eye
parses the last param line expecting another one and gets misdirected to the
ending paren unexpectedly. Where'd the last-last param go? Oh, there isn't
one? Hm, okay. Weird.

Don't forget that this extra comma is only required IF you format your code in
this style. Obvious response is "well don't format it that way". My obvious
response to that would be "Screw you. I'll format my code how I think my code
should be formatted and how I want to read it. Your idiotic lexer hacks to
elide semicolons are getting in my way." After coding for 20+ years with
semicolons I have no objections to them and it's just second nature at this
point to type them in anyway.

(Side-note: Yes I'm only 30 years old and yes I've been coding for 20+ years
since I was 8 years old. [Deal with it](http://www.reactiongifs.com/wp-
content/uploads/2013/01/jack-deal.gif).)

Go lacks a native `enum` type. Its replacement is the somewhat less obvious
combination of a `type` declaration with a `const` section that describes a
series of constant values outside the namespace of that new named type that
should act as the enum's type name. Here's an example:

    
    
    type sortBy int
    const (
        sortByName sortBy = iota
        sortByDate
        sortBySize
    )
    

All that code just to effectively create an enum named `sortBy` that would've
been this brief in C# or Java or C++:

    
    
    enum sortBy {
        Name,
        Date,
        Size
    }
    

Of course we could make both of those even more brief, but the comparison here
is fair I think. The Go version is needlessly more wordy for this most common
of cases. Granted, I like the `iota` concept. That's really cool, but there's
no reason that we can't get `iota` into a native `enum` type in Go.
Furthermore, the lack of the namespace for the enum members means that they
end up at your package level with pseudo-namespace identifiers which makes
things get a bit wordy. At that point you might as well just go back to
writing C code with ENUMNAME_MACROS_LIKE_THIS to define enum members.

There's the horrid syntax of `map[K]V`. This just makes my eyes bleed, but
given the present lack of generics and the inability to design anything less
ugly I guess I'll deal with it. I just can't bring myself to type that in here
again, so let's just move on.

Why is `len` a built-in global function and not a built-in method on
slice/array types? `len(slice)` could just as easily be `slice.Length()` but
it's not. Granted, my syntax is longer, but is obviously more consistent in
appearance with other method calls.

I do like Go's slice support, but I think they didn't take it far enough. They
should've taken a leaf from Python's book and implemented negative end values
to denote positions from the end of the slice instead of having to compute
that offset yourself. The D programming language almost got there with its `$`
token to represent the length of the slice e.g. `a[0 .. $ - 1]`, but I think
I'll give the bronze to Python here for `a[0:-1]`. Go has neither, and forces
you to `a[0 : len(a) - 1]`.

The simpleton will say, "but what's wrong with that?" And I will reply, "Fine,
then try this `package.GetSomething(lots of parameters here)[0 :
len(package.GetSomething(lots of parameters here) - 4]`." Did you get lost?
Did you recompute something there that you shouldn't have? Sure you can just
pull it out to a separate variable on the line above and refactor the entire
expression you just cooked up. Or you could just say
`package.GetSomething(lots of parameters here)[0 : -4]` and you're done.

Now if you're a Go expert and you know something that I don't about this, then
it's not in the (rather terse) language specs. I checked.

**Interfaces:**

I think the most confusing part of the language is that interface
implementation is entirely implicit and not discoverable at all. At first I
thought this would be kind of cool, but unless you're intimately familiar with
all implementation details of all packages, you're never going to know what
interfaces a given type implements. This makes using the standard library a
nightmare.

Okay, this method wants a `Reader` ... do I have a `Reader` here? What is
that? Oh geez, now I have to look at the type the library exposed to me to
check if it even implements that interface... Oh of course it doesn't state it
obviously anywhere so I have to read their source code or gleam that fact by
glancing at ALL their exported methods for ALL their types. If my human-eye
parser is off by a token or two then whoops! I guessed wrong. Oh, that
interface accepts a POINTER to that type but not a copy of.

All this is fine, of course, but Go(d) forbid you have a **dirty import
list**! THE HORROR! How could you not know that you don't need that `time`
package despite the fact that the `os.FileInfo` has a `ModTime()` that gives
you back a `time.Time` that may or may not require you to use the format
string constant from the `time` package!? If you don't need that format string
then you don't need the `time` package and you're a bad developer for
importing it as a precaution. Oh wait, now you _do_ need that format string
constant? Well, you should've imported that `time` project! What's wrong with
you?

Let's not forget about the fact that `interface{}` is the preferred way to
represent the `any` type. Which makes me wonder... WHY NOT JUST ALIAS IT AS
`any` AND BE DONE WITH IT? I don't want to type `interface{}` everywhere when
I could just as easily type `any`. Save the pinkies!

I do understand why that is done and it is pretty cool that the language lets
you just embed an unnamed type declaration where a type is required (unless
that is false which makes this whole justification section moot), but why not
just alias that awful syntax to something much simpler and more meaningful?
The fact that `interface{}` is the catch-all interface is cute and all, but I
don't think we need to encode that fact directly in that representation
throughout all code.

**Standard Library** :

The terminology present in the standard library is just foreign and awkward.
Let's take a few examples:

`html.EscapeString`.
[Escape](http://24.media.tumblr.com/fe0c67dd3b7e5fe6d0fe4226fc71d126/tumblr_mhvliyb1F51s410g9o1_500.gif)?
No, we're ENCODING HTML here, not escaping. HTML has its own encoding. It is
not a string literal to have certain characters escaped with escape
characters, like a `"C \"string\" does with the \\ backslash escape char"`.
HTML is a different language, not an escaped string. Point made? Okay, moving
on.

`net.Dial`. [Dial](http://i.imgur.com/FUzzZ31.gif)? I haven't heard "dial" in
serious use since the good old days of dialing into BBSes with my 57.6k baud
modem (if I was even lucky enough to get that baud rate). "Hello, operator?
Can you dial a TCP address for me? My fingers are too fat to mash the keypad
with." Nowadays we just "Connect" to things. Try to keep up.

`rune` for characters? [What? No. No. No. No no
no.](http://i.imgur.com/kOmtd.gif) Why not `char` LIKE EVERY OTHER LANGUAGE ON
THE PLANET? What new value does the term "rune" bring to the table other than
to just be obscuritan and contrarian like with your usage of `nil`? My
keyboard here does not carve runes into stone tables for archaeologists to
unearth and decipher 2,000 years from now. My keyboard is for typing
characters. Let's get with the times here.

Then there's the complete lack of support for null strings in the JSON
_encoder_. Really? You can't call that a JSON encoder in my book. This means
that you have to design your JSON-friendly structs to have `interface{}` where
you really just mean a `string` that could sometimes be null? Awful.

Pile on top of that the idiotic uppercase-letter-means-public decision and you
get this rule: "The `json` package only accesses the exported fields of struct
types ( **those that begin with an uppercase letter** ). Therefore only the
exported fields of a struct will be present in the JSON output." (emphasis
added). That's quoted right from the [JSON
documentation](http://golang.org/doc/articles/json_and_go.html).

**Pros:**

Let me point out some of the features that I really enjoy so that we don't end
on a completely negative note here.

First, the runtime is extremely solid. I haven't had my HTTP server process
that I wrote in Go go down at all, even when it's faced with boneheaded
developer mistakes. I think that says a lot. Good on you guys for a rock solid
implementation.

The concurrency model is solid. I don't have much experience with channels
yet, but that's definitely the right direction to go. I am getting the
benefits of the concurrency model with `http.Serve` and friends without even
having to explicitly deal with it in my code at all. I like that. Keep it up.

The multi-valued-return functions are awesome and reduce a lot of unnecessary
control flow boilerplate. Combined with the pragmatic `if` statement, there's
definitely power there, e.g. `if v, err := pkg.GetSomething(); err != nil {
yay! }`.

Raw string literals are just great. No more really needs to be said here. I
like that the back-tick **character** (not rune) was used for these strings.
C# did well enough with `@"raw string literals"` but the double quote is such
a common character that you have to double-up on them to escape them, e.g.
`@""""`. I definitely prefer ``back-ticks``. I'm much less likely to require a
literal back-tick character in my strings than a double quote character.

Implicit typing is wonderful with the `:=` operator.

Multi-valued assignment is simply awesome, e.g. `a, b = b, a` to implement a
simple a, b swap operation. I need to take more advantage of that in my code.

The lack of required parens for the `if` statement is great but comes at a
high cost of requiring that the statement body be surrounded in curly-braces
in all cases. This restriction is a bit annoying for simple for-loop `if
(filterout) continue;` cases.

Grouping function parameters by type is awesome, e.g. `func Less(i, j int)`

The name-type order rule contrary to the more common type-name rule is a
welcome change, e.g. `i int` vs. `int i`.

I do agree with Go's explicit error handling strategy via multi-return values
and `if` statements. I'm mostly against exceptions and their ubiquitous use of
handling all error cases. From a reliability standpoint, explicit error
handling is far easier to deal with than a virtually unbounded set of
exceptions that I can't easily reason about.

**Summary** :

Once you get past the warts and big issues and find the workarounds, you can
get really productive in this little language. I am mostly impressed at this
point and want to see bigger and better things. So far, it's the best option I
have for writing reliable network services with, HTTP or otherwise, and having
them execute efficiently.
---
author: jaymz
categories:
- Uncategorized
date: '2012-01-25T17:58:28'
tags: []
title: Google Calendar API access with 2LO (2-legged OAuth) and .NET
---
Getting this scenario to work has been one of the most frustrating experiences
of my development career as yet. I'm writing this incredibly detailed and
informative blog post to save others from self-inflicted premature baldness.
Your scenario is that you want to write a background task or service in .NET
to communicate with Google's servers specific to your Google Apps domain for
business or education. Your service will not have any external exposure to
end-users of your apps domain. You want to synchronize data in bulk to Google
on behalf of your apps domain users. You want your service to use the latest
and greatest APIs that Google advertises that are not deprecated. At the time
of this writing that is the V3 API for things like Calendar
(<http://code.google.com/apis/calendar/v3/getting_started.html>). I will use
the Calendar API as my example here since that's what I was first interested
in using for my project. Google seems to want to make this relatively simple
scenario unnecessarily difficult to find any information on how to do it
_correctly_. All of the documentation does one of the following: (a)
ultimately redirects you to a page talking about a V2 API, not a V3 API, (b)
does not talk at all about 2LO and instead obsesses over 3LO, (c) is
misleading and woefully incomplete with regards to the practical information
you need to know to avoid the dreaded **401 Unauthorized** response from the
API servers. Let me assume that you have created a Google Apps domain for
business or education already and that you have superadmin access to your
Google Apps domain. If you do not, please do so and find the responsible
parties in your organization to grant you superadmin access (if only to a
development-specific domain). **Required set-up steps:** (every single one is
**critical** ; do not skip one thinking you know better or you will fail)

  1. Create an API Project at [https://code.google.com/apis/console](https://code.google.com/apis/console) while logged in as your superadmin account for your apps domain (honestly I'm not sure if it matters which user account you create the project with but it doesn't hurt to be consistent here just in case).
  2. Go to the **API Access** section.
  3. Create an OAuth 2 client named "whatever you want" at "https:// wherever you want; it doesn't matter for this scenario". 
    1. Note that you **don't** need to specify an icon and that the name of your client doesn't matter as no other living soul (a.k.a. end-user in your apps domain) will ever see it.
  4. Copy down the generated client ID ( **somenumberhere.apps.** **googleusercontent.com** ) and the client secret ( **lots-oflettersanddigits-here** ).
  5. Go to the **Services** section while still in the APIs Console and enable your specific services (e.g. Calendar).
  6. Open https://www.google.com/a/cpanel/ **yourdomainhere.com** /ManageOauthClients (need superadmin rights to your google domain here)
  7. Add the client ID ( **somenumberhere.apps.** **googleusercontent.com** ) from step #4 and specify "https://www.google.com/calendar/feeds/" for your scope (assuming you want to work with Calendar API) and click **Authorize**. 
    1. For other APIs, list the proper scopes here, comma-delimited. I just need Calendar API and this works for me. Be sure to specify **https** , not just http.


  8. Go to https://www.google.com/a/cpanel/ **yourdomainhere.com** /SetupOAuth
  9. **Un-** check the "Allow access to all APIs" under "Two-legged OAuth access control". 
    1. Yes, **un-check** it. This is per [http://groups.google.com/ group/google-tasks-api/msg/c8dd0ac7c8f320dc](http://groups.google.com/group/google-tasks-api/msg/c8dd0ac7c8f320dc). As of 2012-01-25, this is still relevant and required. Perhaps this will change in the future but for now it is required.
  10. Save changes.

Now you should have a properly set-up apps domain and API project and the two
are linked together. Let's move on now to your .NET code that will be your
task/service that runs in the background on behalf of your users in your apps
domain. Firstly, I do not recommend using the open-source Google client
library for .NET. I've had bad experiences with it, namely that it has been
known to leak memory. The issue I reported with them on this matter was
claimed to be resolved but I haven't been back to check it out. I had to make
progress on my project and waiting for them to resolve the issue was not an
option. I wrote my own T4 template (Visual Studio code generator) to generate
a client library for Google (and other RESTful APIs which use OAuth 1.0 or
2.0) that has no memory leaks and is both ridiculously fast and efficient:
<https://github.com/JamesDunne/RESTful.tt>. It supports both synchronous and
asynchronous methods of I/O. Its code is up to date as of 2012-11-12. Check
out the project locally and open the TestClient project's Program.cs file.
This is a simple console application designed to demonstrate the simplicity of
the code-generated API client for Google and using OAuth 1.0 for shared-secret
authentication.

    
    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using WellDunne.REST;
    using WellDunne.REST.Google;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    
    namespace TestClient
    {
        class Program
        {
            static void Main(string[] args)
            {
                // Create the authentication module to use oauth 1.0 2LO:
                // NOTE: Replace 'key' and 'secret' with your testing parameters, else you'll get a 401 (Unauthorized) response.
                var oauth = new OAuth10("myclientidhere.apps.googleusercontent.com", "mysecretkeyhere");
    
                // Create the client against googleapis.com:
                // NOTE: This client instance is reusable across threads.
                var client = new WellDunne.REST.Google.V3.CalendarServiceAsyncClient(new Uri("https://www.googleapis.com"), oauth);
    
                // Create the request to get the user's calendar list:
                // NOTE: Each request object is NOT reusable.
                var req = client.GetMyCalendarList(null, null, null, /*requestorID:*/ "username@example.com");
    
                // Fetch the request synchronously:
                var rsp = req.Fetch();
    
                // Write the response JSON object to Console.Out:
                using (var conWriter = new JsonTextWriter(Console.Out))
                    rsp.Response.WriteTo(conWriter);
                Console.WriteLine();
            }
        }
    }

**NOTE:** This test program is specific to Google Calendar. If you are working
with a different API, you'll have to edit the RESTful/Google/Restful.tt T4
template to declare the API methods you need access to. It couldn't hurt to
define some Newtonsoft.Json-enabled classes to deserialize the response data
to. For Google Calendar API testing, simply paste in the values from your API
Console ( **client ID** and **client secret** ) into the `new OAuth10("
**myclientidhere**.apps.googleusercontent.com", " **mysecretkeyhere** ")`
expression. Then paste in an actual user provisioned in your domain into the
`client.GetMyCalendarList(null, null, null, /*requestorID:*/ "
**username@example.com** ")` expression. Run the program and you should see a
raw JSON dump of the response retrieved from Google. For example, I get this
output (id and summary are sanitized):

    
    
    {"kind":"calendar#calendarList","etag":"\"bt6uG7OvVvCre70u9H5QXyrDIXY/5P7Dh-jUGpT56O5EBhfgecrj2pU\"","items":[{"kind":"calendar#calendarListEntry","etag":"\"bt6uG7OvVvCre70u9H5QXyrDIXY/HstY1Kh3cCrbvmn0afdroRd44BQ\"","id":"owner@example.org","summary":"owner@example.org","timeZone":"America/Los_Angeles","colorId":"15","backgroundColor":"#9fc6e7","foregroundColor":"#000000","selected":true,"accessRole":"owner","defaultReminders":[{"method":"email","minutes":10},{"method":"popup","minutes":10}]}]}
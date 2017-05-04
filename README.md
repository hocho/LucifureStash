# LucifureStash

**In addition to supporting most of the features in the existing Azure Storage Client, Lucifure Stash Version 1.3.0,  a .NET client library written in F#, adds the following capabilities in an elegant and intuitive manner. Also available on NuGet.**

- Stash strings and byte arrays larger than 64K.
- Stash arrays and lists – one collection element per table property.
Dynamic stashing via a dictionary, so you do not have to map every table property to a class member.
Makes it trivial to read an existing tables of unknown schema.
Makes merging simple and efficient.   
Morph any data type to a native azure table supported type.
Built in morphing for all enumerations.
Intrinsic morphing for byte, sbyte, char, int16, uint16, uint32 and uint64.
Out of the box morphing using the data contract serializer.
Add you own custom morphing capabilities. Example usage - encryption, compression etc.
Stash public and private; fields and properties.  
Unrestricted table columns names - not tied to the type member names.
Static and dynamic table naming. Generate the table name based on data being stashed to gain seamless  table level partitioning capabilities.
Key names not tied to PartitionKey and RowKey. Name them whatever make the most sense in your domain.  Makes for much more readable LINQ queries.
Powerful abstractions for composite partition and row keys.  
Explicit ETag support for easy control of updates.
Supports both context and context free paradigms for an elegant coding experience. 
Thread safe. Create a single context free client for a table once, and use it across multiple threads. No need to  create new client contexts for every interaction.
Inheritance free object model; no need to inherit your entities from a base class. (Unless you want too.)
Flexible configuration options for easy setup.
Consistent and developer friendly exception messages.
Development time support
Hooks to peek into the http request and response. Great for development time debugging purposes.  Great from runtime logging. 
 Enhanced error reporting quickly points to type definition errors upfront, before calling table storage.
 

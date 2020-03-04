# ChangeLog

**1.2.6.12**
   - Tweak to force association end roles to be correct when roles or multiplicities change
   - Fixed background color on attribute glyph in model explorer
   - Fixed foreground color on enum value glyph on design surface
   - Hid comments in model explorer because they just cluttered up the tree.
   - **[NEW]** Associations now show up in their own compartment in a class on the design surface. Note that this changes the height of your elements, so the first time opening a model you may have to tweak your esthetics a bit.
   - **[NEW]** Double-clicking a class or enum on the designer opens the generated code file, if it exists. If it doesn't exist, you're asked if you'd like to generate the model then, if you do, it tries again.
      - *Known issue:* EFCore/.NETCore and EFCore/.NETFramework projects won't ask to generate the code if they can't open the file; they just fail silently. All other EF/.NET combinations behave appropriately.

**1.2.6.7**
   - An entity's concurrency token property is no longer a required parameter in its constructor (https://github.com/msawczyn/EFDesigner/issues/24)
   - Simplified cascade delete settings in property editor for associations
   - Fixed bad code generation in EFCore for cascade delete overrides (https://github.com/msawczyn/EFDesigner/issues/22)
   - Missing files when generating code for .NET Core projects fixed
   - Tightened up and swatted some bugs in INotifyPropertyChanged handling. Added documentation to doc site for this feature (following up on https://github.com/msawczyn/EFDesigner/issues/23)
   - Ensured multiline editing was available in property window for those properties that made sense

**1.2.6.6**
   - Deleting a generalization or superclass gives the choice of pushing attributes and associations down to the former child class(es)

**1.2.6.5**
   - Comment elements now wrap the text
   - Multiline editor available in property window for element comment descriptions and Comment element text
   - Xml format changed for .efmodel file - can't be loaded by any version < 1.2.6.3
   - Support for automatic migration to new model xml formats

**1.2.6.2**
   - Added XML docs to DbContext, DatabaseInitializer and DbMigrationsConfiguration
   - Enabled drag and drop reordering of enum values and class properties
   - Gave some color to the enum value glyph in the model explorer - it was so boring!
   - Class properties and enum values with warnings now show a warning icon on the design surface
   - Design surface has a property to turn on or off the display of the warning icons 
   - Recategorized a few "Misc" properties on the design surface

**1.2.5.1**
   - Addressed [issue #20 - Abstract/inherited/TPC =code still there for abstract class](https://github.com/msawczyn/EFDesigner/issues/20). While the
     discussion centered around abstract classes and TPC inheritance (which was behaving properly), it did uncover a problem with code generation when
     namespaces changed from class to class. 

**1.2.5.0**
   - Fix for [issue #19 - Recognize "Id" as primary key on import](https://github.com/msawczyn/EFDesigner/issues/19)

**1.2.4.0**
   - Retargeted immediate error and warning messages to Visual Studio output window rather than error window so they could be cleared
   - Added drag validation to Generalization (inheritance) tool
   - Automatically propagate enum name and value name changes to classes that use them

**1.2.3.3**
   - Reverted the selection of the node in the model explorer when an element is selected in the diagram. Was causing bad user experience.
   - Fix for bad code generation when a class has multiple properties that each have an darabase index specified.

**1.2.3.0**
   - When element selected in model explorer, no longer highlights in orange but instead selects, centers and zooms the element.
     This was done because the color change flagged the model as modified, making the user either undo or save the changes to keep
     source control happy.
   - Selecting an element in the diagram also selects it in the model explorer
   - Fix for [issue #12 - Cascade delete](https://github.com/msawczyn/EFDesigner/issues/14). Added another enum value for delete behavior (now is Cascade, None and Default)
     and changed code generation to force no cascade delete if set to 'None' ('None' used to mean 'Use the default behavior', which is now, more explicitly, the 'Default'
     option).
   - Fix for [issue #13 - Unique index not generated in EF6](https://github.com/msawczyn/EFDesigner/issues/13).
   - Fix for [issue #14 - Table with two Primary keys not generated properly in context](https://github.com/msawczyn/EFDesigner/issues/14). Many thanks to @Falthazar!
   - Fix for [issue #18 - Adds ValueGeneratedNever if identity type is Manual](https://github.com/msawczyn/EFDesigner/pull/18). Again, hats off to @Falthazar!

**1.2.2.0**

   - Fix issue with association role end changing without the other side autoatically changing
   - Fix issue with deleting a highlighted element throwing an error when trying to save the file
   - Fixed code generation for dependent classes
   - Designer now automatically saves before generating code

**1.2.1.0**

   - Bug fix for inheritance strategy automatically changing to table-per-hierarchy if targeting EF Core
   - Updated a few warning and error messages to make them more meaningful
   - Fixes for how dependent types work
   - Remove stale error and warnings prior to save (still a few left hanging around that need looked at)
   - Fixed a few null reference errors

**1.2.0.0**

   - **New Features**
      - Roslyn-based code analysis now allows dragging C# files onto the design surface to add or update classes and enums
      - Can add `INotifyPropertyChanged` interface and implementation for entities
      - Ability to tag model as a specific EF version (especially useful for EF Core as new capabilities are being added often)
      - Support for dependent (complex/owned) types 
      - Option to generate dependent types in a separate directory
      - Output directory overrides for classes and enums
      - On model save, can optionally automatically install EF NuGet packages for the model's EF type and version
      - Context menu action to expand and collapse selected classes and enums 
   - **Enhancements**
      - Added ability to add/edit enum values via text in the same way properties can be added/edited in classes
      - Property grid hides element properties if they're not appropriate for the EF version
      - Inheritance strategy automatically changes to table-per-hierarchy if targeting EF Core
      - Context property `Database Type` changed to `SqlServer Type` to better reflect what it does
      - Selecting an element in the Model Explorer highlights it on the diagram

**1.1.0.0**

   - Bug fixes for exceptions thrown when bad input to model attributes as text
   - Added MinLength string property (used in EF6 only as of this writing)
   - Modified attribute parser to accept MinLength
   - Added ColumnName property to model attribute
   - Added [MEF extension capability](https://docs.microsoft.com/en-us/visualstudio/modeling/extend-your-dsl-by-using-mef)
   - Added some unit tests
   - Added some documentation updates
   - Changed version to 1.1.0 due to MEF capability

**1.0.3.9**

   - If no entities and model is using an unsupported inheritance strategy, 
     changing from EF6 to EFCore doesn't give a message, just changes the strategy.
   - Added IsFlags attribute (and matching validations and behavior) to Enums
   - NGENed extension assembly

**1.0.3.8**

   - Fixed project item placement
   - Added change checks to diagram so dirty flag doesn't set when nothing changes

**1.0.3.7**

   - Emergency bug fixes

**1.0.3.6**

   - Fixed parser errors when editing model attributes as text
   - Fixed error when auto-generating on save and design surface is not the active window
   - Fixed crash when used on non-English-language systems (where Microsoft Pluralization Service is unavailable)
   - Added option to generate warnings if no documentation
   - Standardized warning and error message structure
   - Added ability to choose 'None' DatabaseInitializer type; generates SetInitializer(null)

**1.0.3.5**

   - Enhanced portability between EF6 an EFCore

**1.0.3.4**

   - Adds some T4 fixes to make generated code more usable in ASP.NET Core applications. 
   - Fix to spurious error when copying/pasting enum elements.
   - First release that's available on Visual Studio Marketplace.

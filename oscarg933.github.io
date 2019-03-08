Skip to content
Your account has been flagged.
Because of that, your profile is hidden from the public. If you believe this is a mistake, contact support to have your account status reviewed.
roslynator
Repositories3
Code2K
Commits116
Issues264
Marketplace0
Topics0
Wikis5
Users0
Language

Sort

2,869 code results
@hvdthong
hvdthong/RepoExtractor – JosefPihrt.Roslynator.md
Showing the top three matches
Last indexed on Aug 6, 2018
Markdown
# Roslynator <img align="left" width="48px" height="48px" src="http://pihrt.net/images/Roslynator.ico">
* [List of code fixes for CS diagnostics](src/CodeFixes/README.md)
* [Release notes](ChangeLog.md)

### Donation

Although Roslynator products are free of charge, any [donation](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=BX85UA346VTN6) is welcome and supports further development.
@JosefPihrt
JosefPihrt/SnippetManager – GenerateDocumentation.targets
Showing the top two matches
Last indexed on Oct 14, 2018
XML
<Project>
  <Target Name="GenerateDocumentation" AfterTargets="RoslynatorInitialize" Condition=" '$(Configuration)' == 'Release'">

    <PropertyGroup>
      <RoslynatorAssemblies>Pihrtsoft.Snippets.dll</RoslynatorAssemblies>
@kibiz0r
kibiz0r/dotfiles – install_vscode_roslynator
Showing the top three matches
Last indexed on Sep 23, 2018
Shell
#!/usr/bin/env sh

if [ -d omnisharp/Roslynator.VisualStudio ]; then
  echo "Roslynator already exists"
else
  curl -Lo Roslynator.VisualStudio.vsix https://josefpihrt.gallerycdn.vsassets.io/extensions/josefpihrt/roslynator2017/1.9.0/1528893949710/Roslynator.VisualStudio.1.9.0.vsix
@SeanLatimer
SeanLatimer/I3DMappingExporter – I3DMapper.csproj
Showing the top two matches
Last indexed on Dec 20, 2018
XML
  </ItemGroup>
  <ItemGroup>
    <Analyzer Include="..\packages\Roslynator.Analyzers.2.0.0\analyzers\dotnet\cs\Roslynator.Common.dll" />
    <Analyzer Include="..\packages\Roslynator.Analyzers.2.0.0\analyzers\dotnet\cs\Roslynator.Common.Workspaces.dll" />
@mbwtepaske
mbwtepaske/HPF – HPF.ruleset
Showing the top two matches
Last indexed on Jul 10, 2018
<RuleSet Name="New Rule Set" Description=" " ToolsVersion="15.0">
  <Rules AnalyzerId="Roslynator.CSharp.Analyzers" RuleNamespace="Roslynator.CSharp.Analyzers">
@JosefPihrt
JosefPihrt/LinqToRegex – GenerateDocumentation.targets
Showing the top two matches
Last indexed on Oct 14, 2018
XML
<Project>
  <Target Name="GenerateDocumentation" AfterTargets="RoslynatorInitialize" Condition=" '$(Configuration)' == 'Release'">

    <PropertyGroup>
      <RoslynatorAssemblies>Pihrtsoft.Text.RegularExpressions.Linq.dll</RoslynatorAssemblies>
@kzu
kzu/roslynator – Roslynator.sln
Showing the top six matches
Last indexed on Jul 8, 2018
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "Roslynator.Implementation", "src\Roslynator.Implementation\Roslynator.Implementation.csproj", "{9CCD8D09-EA2E-4818-A1A6-6AC9D068EBB4}"
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "Roslynator.Interfaces", "src\Roslynator.Interfaces\Roslynator.Interfaces.csproj", "{77231B6B-E213-4450-9A22-81C86BD2C8F8}"
@GermanKuber
GermanKuber/EmotivCustom – EmotivCustom.Core.csproj
Showing the top two matches
Last indexed on Jul 11, 2018
XML
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Roslynator" Version="0.2.10" />
    <PackageReference Include="Roslynator.Analyzers" Version="1.6.0-rc" />
  </ItemGroup>
@GermanKuber
GermanKuber/EmotivCustom – EmotivCustom.Sdk.csproj
Showing the top two matches
Last indexed on Jul 11, 2018
XML
  <ItemGroup>
    <PackageReference Include="Roslynator" Version="0.2.10" />
    <PackageReference Include="Roslynator.Analyzers" Version="1.6.0-rc" />
@jwooley
jwooley/RoslynAndYou – LanguageFeaturesCS.csproj
Showing the top two matches
Last indexed on Oct 11, 2018
XML
    <Analyzer Include="..\packages\Microsoft.CodeAnalysis.Analyzers.1.1.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.Analyzers.dll" />
    <Analyzer Include="..\packages\Roslynator.Analyzers.1.9.0\analyzers\dotnet\cs\Roslynator.Common.dll" />
© 2019 GitHub, Inc.
Terms
Privacy
Security
Status
Help
Contact GitHub
Pricing
API
Training
Blog
About
Press h to open a hovercard with more details.

export const configurationFileContent = {
	ruleset: `<?xml version="1.0" encoding="utf-8"?>
<!--

This rule set can be used to:

 1) Enable/disable analyzer(s) by DEFAULT.
 2) Change DEFAULT severity (action) of the analyzer(s).
 
Default configuration is applied once when analyzers are loaded.
Therefore, it may be necessary to restart IDE for changes to take effect.

Although it is possible to edit ruleset manually, Visual Studio has built-in support for editing ruleset.
Just add ruleset file to a solution and open it.

-->
<RuleSet Name="roslynator.ruleset" ToolsVersion="16.0">
  
  <!-- Specify default action that should be applied to all analyzers except those explicitly specified. -->
  <!-- <IncludeAll Action="None,Hidden,Info,Warning,Error" /> -->
  
  <!-- Specify zero or more paths to other rulesets that should be included. -->
  <!-- <Include Path="" Action="Default,None,Hidden,Info,Warning,Error" /> -->

  <Rules AnalyzerId="Roslynator.CSharp.Analyzers" RuleNamespace="Roslynator.CSharp.Analyzers">

    <!-- Specify default action that should be applied to a specified analyzer. -->
    <!-- <Rule Id="RCS...." Action="None,Hidden,Info,Warning,Error" /> -->

  </Rules>

</RuleSet>`,
	config: `<?xml version="1.0" encoding="utf-8"?>
<Roslynator>
  <Settings>
    <General>
      <!-- <PrefixFieldIdentifierWithUnderscore>true</PrefixFieldIdentifierWithUnderscore> -->
    </General>
    <Refactorings>
      <!-- <Refactoring Id="RRxxxx" IsEnabled="false" /> -->
    </Refactorings>
    <CodeFixes>
      <!-- <CodeFix Id="CSxxxx.RCFxxxx" IsEnabled="false" /> -->
      <!-- <CodeFix Id="CSxxxx" IsEnabled="false" /> -->
      <!-- <CodeFix Id="RCFxxxx" IsEnabled="false" /> -->
    </CodeFixes>
  </Settings>
</Roslynator>`
};
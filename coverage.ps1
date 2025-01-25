$coverageResults = 'CoverageResults';
$testResults = 'TestResults';

rm -Recurse -Force ".\${coverageResults}" -ErrorAction SilentlyContinue;
rm -Recurse -Force ".\${testResults}" -ErrorAction SilentlyContinue;

dotnet test --collect:"XPlat Code Coverage" --results-directory:"$testResults"

reportgenerator -reports:".\${testResults}\*\coverage.cobertura.xml" -targetdir:"$coverageResults" -filefilters:"-*Microsoft.Extensions.Logging.Generators\Microsoft.Extensions.Logging.Generators.LoggerMessageGenerator\LoggerMessage.g.cs;-*System.Text.RegularExpressions.Generator\System.Text.RegularExpressions.Generator.RegexGenerator\RegexGenerator.g.cs"

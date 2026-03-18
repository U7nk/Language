using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Emit;

public static class EmittingShared
{
      public static string GetIRForType(TypeSymbol typeSymbol)
      {
            if (Equals(typeSymbol, TypeSymbol.BuiltIn.Bool())) 
                  return ("i1");
            else if (Equals(typeSymbol, TypeSymbol.BuiltIn.Int())) 
                  return ("i32");
                
            else if (Equals(typeSymbol, TypeSymbol.BuiltIn.String())) 
                  return ("ptr");
                
            else if (Equals(typeSymbol, TypeSymbol.BuiltIn.Object())) 
                  return ("ptr");
            else if (Equals(typeSymbol, TypeSymbol.BuiltIn.Void())) 
                  return ("void");
            else
                  return ("ptr");
      }
      
      public static string GetMethodName(MethodSymbol methodSymbol, bool addQuotesAndAtSign = false)
      {
            if (addQuotesAndAtSign)
            {
                  return $"@\"{methodSymbol.ContainingType.Unwrap().GetFullName()}.{methodSymbol.Name}\"";
            }
            
            return $"{methodSymbol.ContainingType.Unwrap().GetFullName()}.{methodSymbol.Name}";
      }
}
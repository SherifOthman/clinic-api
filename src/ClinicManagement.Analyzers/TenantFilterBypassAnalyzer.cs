using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ClinicManagement.Analyzers;

/// <summary>
/// Roslyn analyzer that flags direct calls to IgnoreQueryFilters() outside
/// of allowed namespaces (Admin repositories and background jobs).
///
/// Rule: IgnoreQueryFilters must NEVER appear in:
///   - Application/Features/** (handlers, queries, commands)
///   - Any namespace that does not contain "Admin" or "Jobs" or "Seeders"
///
/// Allowed namespaces (whitelist):
///   - ClinicManagement.Persistence.Repositories (admin methods only — enforced by naming convention)
///   - ClinicManagement.Persistence.Jobs
///   - ClinicManagement.Persistence.Seeders
///   - ClinicManagement.Persistence.Security (TenantGuard itself)
///
/// Diagnostic: CM001 — Direct IgnoreQueryFilters call outside allowed context
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TenantFilterBypassAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "CM001";

    private static readonly DiagnosticDescriptor Rule = new(
        id:                 DiagnosticId,
        title:              "Direct IgnoreQueryFilters call outside allowed context",
        messageFormat:      "IgnoreQueryFilters() called directly in '{0}'. Use TenantGuard.AsAdminQuery() or TenantGuard.AsSystemQuery() instead. Direct calls are only allowed in TenantGuard itself.",
        category:           "Security",
        defaultSeverity:    DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description:        "Direct IgnoreQueryFilters() calls bypass tenant isolation. Use TenantGuard wrappers to ensure cross-tenant access is intentional and auditable.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check if the method name is IgnoreQueryFilters
        var methodName = invocation.Expression switch
        {
            MemberAccessExpressionSyntax m => m.Name.Identifier.Text,
            IdentifierNameSyntax i         => i.Identifier.Text,
            _                              => null
        };

        if (methodName != "IgnoreQueryFilters") return;

        // Get the containing namespace
        var namespaceName = GetContainingNamespace(invocation);

        // TenantGuard itself is allowed — it's the one place that wraps the call
        if (namespaceName.Contains("ClinicManagement.Persistence.Security")) return;

        // Background jobs and seeders are allowed (system context, no HTTP user)
        if (namespaceName.Contains(".Jobs") || namespaceName.Contains(".Seeders")) return;

        // Any other direct call is a violation
        context.ReportDiagnostic(
            Diagnostic.Create(Rule, invocation.GetLocation(), namespaceName));
    }

    private static string GetContainingNamespace(SyntaxNode node)
    {
        var current = node.Parent;
        while (current != null)
        {
            if (current is NamespaceDeclarationSyntax ns)
                return ns.Name.ToString();
            if (current is FileScopedNamespaceDeclarationSyntax fns)
                return fns.Name.ToString();
            current = current.Parent;
        }
        return "<global>";
    }
}

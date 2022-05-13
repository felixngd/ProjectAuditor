using System.Linq;
using Unity.ProjectAuditor.Editor.AssemblyUtils;

namespace Unity.ProjectAuditor.Editor
{
    class AssemblyDependencyNode : DependencyNode
    {
        string m_Name;

        public AssemblyDependencyNode(string name, string[] deps = null)
        {
            m_Name = name;
            if (deps != null)
                AddChildren(deps.Select(d => new AssemblyDependencyNode(d)).ToArray<DependencyNode>());
        }

        public override string GetName()
        {
            return m_Name;
        }

        public override string GetPrettyName()
        {
            return m_Name;
        }

        public override bool IsPerfCritical()
        {
            return false;
        }
    }
}

using ExamTester.Models;

namespace ExamTester.Services;

public class ExamCatalogService
{
    private readonly List<ExamCatalogEntry> _catalog;
    private readonly PersistenceService _persistence;

    public ExamCatalogService(PersistenceService persistence)
    {
        _persistence = persistence;
        _catalog = BuildCatalog();
    }

    public List<ExamCatalogEntry> GetAllExams()
    {
        return _catalog.Where(e => e.RetiredDate == null).ToList();
    }

    public List<ExamCatalogEntry> GetExamsByVendor(string vendor)
    {
        return _catalog.Where(e => e.Vendor.Equals(vendor, StringComparison.OrdinalIgnoreCase) && e.RetiredDate == null).ToList();
    }

    public List<ExamCatalogEntry> GetExamsByCategory(string category)
    {
        return _catalog.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase) && e.RetiredDate == null).ToList();
    }

    public ExamCatalogEntry? GetExamById(string id)
    {
        return _catalog.FirstOrDefault(e => e.Id.Equals(id, StringComparison.OrdinalIgnoreCase)
            || e.Code.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public List<string> GetVendors()
    {
        return _catalog.Select(e => e.Vendor).Distinct().OrderBy(v => v).ToList();
    }

    public List<string> GetCategories()
    {
        return _catalog.Select(e => e.Category).Distinct().OrderBy(c => c).ToList();
    }

    public List<ExamCatalogEntry> SearchExams(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return GetAllExams();

        var q = query.ToLowerInvariant();
        return _catalog.Where(e =>
            e.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
            || e.Code.Contains(q, StringComparison.OrdinalIgnoreCase)
            || e.Vendor.Contains(q, StringComparison.OrdinalIgnoreCase)
            || e.Description.Contains(q, StringComparison.OrdinalIgnoreCase)
            || e.Tags.Any(t => t.Contains(q, StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }

    private static List<ExamCatalogEntry> BuildCatalog()
    {
        return new List<ExamCatalogEntry>
        {
            // Microsoft Azure
            new ExamCatalogEntry
            {
                Id = "az-900", Code = "AZ-900", Name = "Azure Fundamentals",
                Vendor = "Microsoft", Category = "Cloud",
                Description = "Prove your foundational knowledge of cloud concepts, Azure services, Azure workloads, security and privacy in Azure, as well as Azure pricing and support.",
                QuestionCount = 45, TimeLimit = 65, PassingScore = 70, Difficulty = "Beginner",
                Tags = new List<string> { "azure", "cloud", "fundamentals" },
                Domains = new List<ExamDomain>
                {
                    new() { Name = "Cloud Concepts", Weight = 25, Topics = new() { "Cloud computing benefits", "Cloud service types (IaaS, PaaS, SaaS)", "Cloud deployment models" } },
                    new() { Name = "Azure Architecture", Weight = 35, Topics = new() { "Azure regions", "Availability zones", "Resource groups", "Azure Resource Manager" } },
                    new() { Name = "Azure Services", Weight = 25, Topics = new() { "Compute", "Networking", "Storage", "Databases" } },
                    new() { Name = "Security and Compliance", Weight = 15, Topics = new() { "Network security", "Identity services", "Azure governance" } }
                },
                Objectives = new() { "Describe cloud concepts", "Describe Azure architecture and services", "Describe Azure management and governance" }
            },
            new ExamCatalogEntry
            {
                Id = "az-104", Code = "AZ-104", Name = "Azure Administrator",
                Vendor = "Microsoft", Category = "Cloud",
                Description = "Demonstrate your ability to manage Azure identities and governance, implement and manage storage, deploy and manage compute resources, configure and manage virtual networking, and monitor and maintain Azure resources.",
                QuestionCount = 55, TimeLimit = 100, PassingScore = 70, Difficulty = "Intermediate",
                Tags = new List<string> { "azure", "administrator", "infrastructure" },
                Domains = new List<ExamDomain>
                {
                    new() { Name = "Manage Azure Identities and Governance", Weight = 20, Topics = new() { "Azure AD", "RBAC", "Subscriptions", "Azure Policy" } },
                    new() { Name = "Implement and Manage Storage", Weight = 15, Topics = new() { "Storage accounts", "Blob storage", "Azure Files", "Storage security" } },
                    new() { Name = "Deploy and Manage Compute", Weight = 20, Topics = new() { "VMs", "App Service", "Containers", "Azure Kubernetes Service" } },
                    new() { Name = "Configure and Manage Networking", Weight = 25, Topics = new() { "Virtual networks", "NSGs", "Azure DNS", "Load balancing" } },
                    new() { Name = "Monitor and Maintain Resources", Weight = 20, Topics = new() { "Azure Monitor", "Backup", "Disaster recovery" } }
                },
                Objectives = new() { "Manage Azure identities and governance", "Implement and manage storage", "Deploy and manage Azure compute resources", "Implement and manage virtual networking", "Monitor and maintain Azure resources" }
            },
            new ExamCatalogEntry
            {
                Id = "az-305", Code = "AZ-305", Name = "Azure Solutions Architect Expert",
                Vendor = "Microsoft", Category = "Cloud",
                Description = "Design solutions for identity and governance, data storage, business continuity, and infrastructure in Azure.",
                QuestionCount = 50, TimeLimit = 100, PassingScore = 70, Difficulty = "Advanced",
                Tags = new List<string> { "azure", "architect", "design" },
                Domains = new List<ExamDomain>
                {
                    new() { Name = "Design Identity and Governance", Weight = 25, Topics = new() { "Governance solutions", "Authentication", "Authorization" } },
                    new() { Name = "Design Data Storage", Weight = 25, Topics = new() { "Data storage solutions", "Data integration" } },
                    new() { Name = "Design Business Continuity", Weight = 25, Topics = new() { "Backup solutions", "High availability", "Disaster recovery" } },
                    new() { Name = "Design Infrastructure", Weight = 25, Topics = new() { "Compute solutions", "Network solutions", "Application architecture" } }
                },
                Objectives = new() { "Design identity, governance, and monitoring solutions", "Design data storage solutions", "Design business continuity solutions", "Design infrastructure solutions" }
            },

            // AWS
            new ExamCatalogEntry
            {
                Id = "clf-c02", Code = "CLF-C02", Name = "AWS Cloud Practitioner",
                Vendor = "AWS", Category = "Cloud",
                Description = "Validate overall understanding of the AWS Cloud, including concepts, services, security, architecture, pricing, and support.",
                QuestionCount = 65, TimeLimit = 90, PassingScore = 70, Difficulty = "Beginner",
                Tags = new List<string> { "aws", "cloud", "fundamentals" },
                Domains = new List<ExamDomain>
                {
                    new() { Name = "Cloud Concepts", Weight = 24, Topics = new() { "AWS Cloud value proposition", "Cloud economics", "Cloud architecture design principles" } },
                    new() { Name = "Security and Compliance", Weight = 30, Topics = new() { "Shared responsibility model", "IAM", "Security services" } },
                    new() { Name = "Cloud Technology and Services", Weight = 34, Topics = new() { "Compute", "Storage", "Networking", "Databases" } },
                    new() { Name = "Billing, Pricing, and Support", Weight = 12, Topics = new() { "Pricing models", "Billing", "Support plans" } }
                },
                Objectives = new() { "Define the AWS Cloud and its value proposition", "Identify security and compliance aspects", "Identify core AWS services", "Understand billing and pricing" }
            },
            new ExamCatalogEntry
            {
                Id = "saa-c03", Code = "SAA-C03", Name = "AWS Solutions Architect Associate",
                Vendor = "AWS", Category = "Cloud",
                Description = "Design secure, resilient, high-performing, and cost-optimized architectures on AWS.",
                QuestionCount = 65, TimeLimit = 130, PassingScore = 72, Difficulty = "Intermediate",
                Tags = new List<string> { "aws", "architect", "design" },
                Domains = new List<ExamDomain>
                {
                    new() { Name = "Secure Architectures", Weight = 30, Topics = new() { "IAM", "Encryption", "Network security" } },
                    new() { Name = "Resilient Architectures", Weight = 26, Topics = new() { "Multi-AZ", "Fault tolerance", "Decoupling" } },
                    new() { Name = "High-Performing Architectures", Weight = 24, Topics = new() { "Compute optimization", "Storage optimization", "Database optimization" } },
                    new() { Name = "Cost-Optimized Architectures", Weight = 20, Topics = new() { "Cost-effective resources", "Right-sizing", "Reserved capacity" } }
                },
                Objectives = new() { "Design secure architectures", "Design resilient architectures", "Design high-performing architectures", "Design cost-optimized architectures" }
            },

            // CompTIA
            new ExamCatalogEntry
            {
                Id = "sy0-701", Code = "SY0-701", Name = "CompTIA Security+",
                Vendor = "CompTIA", Category = "Security",
                Description = "Validate baseline security skills and knowledge including threat assessment, network security, compliance, and identity management.",
                QuestionCount = 90, TimeLimit = 90, PassingScore = 75, Difficulty = "Intermediate",
                Tags = new List<string> { "security", "comptia", "cybersecurity" },
                Domains = new List<ExamDomain>
                {
                    new() { Name = "General Security Concepts", Weight = 12, Topics = new() { "Security controls", "Threat actors", "Cryptography concepts" } },
                    new() { Name = "Threats, Vulnerabilities, and Mitigations", Weight = 22, Topics = new() { "Attack types", "Vulnerability types", "Mitigation techniques" } },
                    new() { Name = "Security Architecture", Weight = 18, Topics = new() { "Network architecture", "Secure infrastructure", "Cloud security" } },
                    new() { Name = "Security Operations", Weight = 28, Topics = new() { "Monitoring", "Incident response", "Digital forensics" } },
                    new() { Name = "Security Program Management", Weight = 20, Topics = new() { "Governance", "Risk management", "Compliance" } }
                },
                Objectives = new() { "Assess security posture", "Monitor and secure hybrid environments", "Operate with awareness of applicable regulations", "Identify and respond to security incidents" }
            },
            new ExamCatalogEntry
            {
                Id = "n10-009", Code = "N10-009", Name = "CompTIA Network+",
                Vendor = "CompTIA", Category = "Networking",
                Description = "Validate your ability to design, configure, manage, and troubleshoot wired and wireless network devices.",
                QuestionCount = 90, TimeLimit = 90, PassingScore = 72, Difficulty = "Intermediate",
                Tags = new List<string> { "networking", "comptia", "infrastructure" },
                Domains = new List<ExamDomain>
                {
                    new() { Name = "Networking Fundamentals", Weight = 24, Topics = new() { "OSI model", "Network topologies", "Ports and protocols" } },
                    new() { Name = "Network Implementations", Weight = 19, Topics = new() { "Routing", "Switching", "Wireless technologies" } },
                    new() { Name = "Network Operations", Weight = 16, Topics = new() { "Monitoring", "Documentation", "Business continuity" } },
                    new() { Name = "Network Security", Weight = 19, Topics = new() { "Security concepts", "Attack types", "Mitigation techniques" } },
                    new() { Name = "Network Troubleshooting", Weight = 22, Topics = new() { "Methodology", "Cable issues", "Network software tools" } }
                },
                Objectives = new() { "Explain fundamental networking concepts", "Implement network solutions", "Use best practices to manage the network", "Secure the network", "Troubleshoot network issues" }
            },

            // Cisco
            new ExamCatalogEntry
            {
                Id = "ccna-200-301", Code = "200-301", Name = "Cisco CCNA",
                Vendor = "Cisco", Category = "Networking",
                Description = "Prove your knowledge of network fundamentals, IP connectivity, security fundamentals, automation, and programmability.",
                QuestionCount = 100, TimeLimit = 120, PassingScore = 80, Difficulty = "Intermediate",
                Tags = new List<string> { "cisco", "networking", "ccna" },
                Domains = new List<ExamDomain>
                {
                    new() { Name = "Network Fundamentals", Weight = 20, Topics = new() { "TCP/IP", "Switching concepts", "IPv4/IPv6" } },
                    new() { Name = "Network Access", Weight = 20, Topics = new() { "VLANs", "Interswitch connectivity", "Layer 2 discovery protocols" } },
                    new() { Name = "IP Connectivity", Weight = 25, Topics = new() { "Routing", "First hop redundancy protocols", "IP services" } },
                    new() { Name = "IP Services", Weight = 10, Topics = new() { "NAT", "NTP", "DHCP", "QoS" } },
                    new() { Name = "Security Fundamentals", Weight = 15, Topics = new() { "Key security concepts", "Access control lists", "Wireless security protocols" } },
                    new() { Name = "Automation and Programmability", Weight = 10, Topics = new() { "REST APIs", "Configuration management", "JSON/XML" } }
                },
                Objectives = new() { "Identify network fundamentals", "Configure and verify network access", "Describe IP connectivity", "Configure and verify IP services", "Describe security fundamentals", "Describe automation and programmability" }
            },

            // ISC2
            new ExamCatalogEntry
            {
                Id = "cissp", Code = "CISSP", Name = "Certified Information Systems Security Professional",
                Vendor = "ISC2", Category = "Security",
                Description = "Demonstrate expertise in designing, implementing, and managing a best-in-class cybersecurity program.",
                QuestionCount = 125, TimeLimit = 180, PassingScore = 70, Difficulty = "Expert",
                Tags = new List<string> { "security", "isc2", "cissp", "cybersecurity" },
                Domains = new List<ExamDomain>
                {
                    new() { Name = "Security and Risk Management", Weight = 15, Topics = new() { "Security governance", "Compliance", "Risk management", "Business continuity" } },
                    new() { Name = "Asset Security", Weight = 10, Topics = new() { "Information classification", "Data handling", "Privacy" } },
                    new() { Name = "Security Architecture and Engineering", Weight = 13, Topics = new() { "Security models", "Cryptography", "Physical security" } },
                    new() { Name = "Communication and Network Security", Weight = 13, Topics = new() { "Secure network architecture", "Network components", "Secure communications" } },
                    new() { Name = "Identity and Access Management", Weight = 13, Topics = new() { "Physical and logical access", "Authentication", "Identity as a service" } },
                    new() { Name = "Security Assessment and Testing", Weight = 12, Topics = new() { "Assessment strategies", "Security testing", "Vulnerability analysis" } },
                    new() { Name = "Security Operations", Weight = 13, Topics = new() { "Investigations", "Incident management", "Disaster recovery" } },
                    new() { Name = "Software Development Security", Weight = 11, Topics = new() { "SDLC", "Development security controls", "Software security effectiveness" } }
                },
                Objectives = new() { "Understand and apply security concepts", "Design and manage security architecture", "Manage identity and access", "Assess and test security", "Manage security operations", "Secure software development" }
            },

            // Google Cloud
            new ExamCatalogEntry
            {
                Id = "gcp-ace", Code = "ACE", Name = "Google Cloud Associate Cloud Engineer",
                Vendor = "Google", Category = "Cloud",
                Description = "Deploy and secure applications and infrastructure, monitor operations, and manage enterprise solutions on Google Cloud.",
                QuestionCount = 50, TimeLimit = 120, PassingScore = 70, Difficulty = "Intermediate",
                Tags = new List<string> { "gcp", "google", "cloud", "engineer" },
                Domains = new List<ExamDomain>
                {
                    new() { Name = "Setting Up a Cloud Solution Environment", Weight = 18, Topics = new() { "Cloud projects", "Billing", "CLI configuration" } },
                    new() { Name = "Planning and Configuring a Cloud Solution", Weight = 22, Topics = new() { "Compute resources", "Data storage", "Network resources" } },
                    new() { Name = "Deploying and Implementing a Cloud Solution", Weight = 26, Topics = new() { "Compute Engine", "Kubernetes Engine", "App Engine", "Cloud Functions" } },
                    new() { Name = "Ensuring Successful Operation", Weight = 18, Topics = new() { "Managing resources", "Monitoring and logging" } },
                    new() { Name = "Configuring Access and Security", Weight = 16, Topics = new() { "IAM", "Service accounts", "Audit logging" } }
                },
                Objectives = new() { "Set up a cloud solution environment", "Plan and configure a cloud solution", "Deploy and implement", "Ensure successful operation", "Configure access and security" }
            }
        };
    }
}

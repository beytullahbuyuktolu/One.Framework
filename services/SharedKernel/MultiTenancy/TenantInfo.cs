using System;

namespace SharedKernel.MultiTenancy
{
    public class TenantInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; }
    }

    public interface ITenantService
    {
        TenantInfo GetCurrentTenant();
        void SetCurrentTenant(TenantInfo tenant);
    }

    public class TenantService : ITenantService
    {
        private static readonly AsyncLocal<TenantInfo> _currentTenant = new AsyncLocal<TenantInfo>();

        public TenantInfo GetCurrentTenant()
        {
            return _currentTenant.Value;
        }

        public void SetCurrentTenant(TenantInfo tenant)
        {
            _currentTenant.Value = tenant;
        }
    }
}

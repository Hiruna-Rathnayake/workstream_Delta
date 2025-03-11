using AutoMapper;
using workstream.DTO;
using workstream.Model;

namespace workstream.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Tenant Mappings
            CreateMap<Tenant, TenantReadDTO>();
            CreateMap<TenantWriteDTO, Tenant>();
            CreateMap<TenantWithUserDTO, Tenant>();
            CreateMap<TenantWithUserDTO, User>();

            // User Mappings
            CreateMap<User, UserReadDTO>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role));
            CreateMap<UserWriteDTO, User>();
            CreateMap<UserUpdateDTO, User>();

            // Role Mappings
            CreateMap<Role, RoleReadDTO>();
            CreateMap<RoleWriteDTO, Role>();

            // Permission Mappings
            CreateMap<Permission, PermissionReadDTO>();

            // Inventory Mappings
            CreateMap<InventoryItem, InventoryItemReadDTO>();
            CreateMap<InventoryItemWriteDTO, InventoryItem>();

            // Stock Mappings
            CreateMap<Stock, StockReadDTO>();
            CreateMap<StockWriteDTO, Stock>();

            // Order Mappings
            CreateMap<Order, OrderReadDTO>();
            CreateMap<OrderWriteDTO, Order>();

            // OrderItem Mappings
            CreateMap<OrderItem, OrderItemReadDTO>()
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Price * src.Quantity))
                .ForMember(dest => dest.InventoryItemName, opt => opt.MapFrom(src => src.InventoryItem.Name));

            CreateMap<OrderItemWriteDTO, OrderItem>();

            // Customer Mappings
            CreateMap<Customer, CustomerReadDTO>();  // Model to DTO (Read)
            CreateMap<CustomerWriteDTO, Customer>(); // DTO to Model (Write)

            // Map Role to RoleReadDTO for fetching role details
            CreateMap<Role, RoleReadDTO>();

            // Map RoleWriteDTO to Role for role creation or update
            CreateMap<RoleWriteDTO, Role>();
            CreateMap<Role, RoleWithPermissionsDTO>();


            CreateMap<Permission, PermissionReadDTO>();
        }
    }
}

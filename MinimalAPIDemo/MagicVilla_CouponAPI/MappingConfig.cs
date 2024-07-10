using AutoMapper;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace MagicVilla_CouponAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig() 
        { 
            CreateMap<Coupon,CouponCreateDTO>().ReverseMap();
            CreateMap<Coupon,CouponUpateDTO>().ReverseMap();
            CreateMap<Coupon,CouponDTO>().ReverseMap();
            CreateMap<LocalUser,UserDTO>().ReverseMap();
        }
    }
}

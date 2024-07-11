﻿using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Repository.IRepository;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MagicVilla_CouponAPI.Endpoints
{
    public static class CouponEndpoints
    {
        public static void ConfigureCouponEndpoints(this WebApplication app)
        {
            app.MapGet("/api/coupon", GetAllCoupon)
                .WithName("GetCoupons").Produces<APIResponse>(200);
                //.RequireAuthorization("AdminOnly");

            app.MapGet("/api/coupon/{id:int}", GetCoupon)
                .WithName("GetCoupon").Produces<APIResponse>(200)
                .AddEndpointFilter(async (context, next) =>
                {
                    var id = context.GetArgument<int>(2);
                    if (id ==0)
                    {
                        return Results.BadRequest("Cannot have 0 in id");
                    }
                    //action to do before execution of endpoint
                    Console.WriteLine("Before filter");
                    var result = await next(context);
                    //action to do after execution of endpoint
                    Console.WriteLine("After filter");
                    return result;
                })
                .AddEndpointFilter(async (context, next) =>
                {
                    
                    //action to do before execution of endpoint
                    Console.WriteLine("Before 2nd filter");
                    var result = await next(context);
                    //action to do after execution of endpoint
                    Console.WriteLine("After 2nd filter");
                    return result;
                });

            app.MapPost("/api/coupon/", CreateCoupon)
                .WithName("CreatedCoupon").Accepts<CouponCreateDTO>("application/json").Produces<APIResponse>(201).Produces(400);

            app.MapPut("/api/coupon/", UpdateCoupon).WithName("UpdateCoupon")
                .Accepts<CouponUpateDTO>("application/json").Produces<APIResponse>(200).Produces(400);

            app.MapDelete("/api/coupon/{id:int}", DeleteCoupon);
        }

        private async static Task<IResult> GetCoupon(ICouponRepository _couponRepo, ILogger<Program> _logger, int id)
        {
            Console.WriteLine("Endpoint executed");     
            APIResponse response = new();
            response.Result = await _couponRepo.GetAsync(id);
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }
        //[Authorize]
        private async static Task<IResult> CreateCoupon(ICouponRepository _couponRepo, IMapper _mapper,
                IValidator<CouponCreateDTO> _validation, [FromBody] CouponCreateDTO coupon_C_DTO)
        {
            APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };


            var validationResult = await _validation.ValidateAsync(coupon_C_DTO);
            if (!validationResult.IsValid)
            {
                response.ErrorMessage.Add(validationResult.Errors.FirstOrDefault().ToString());
                return Results.BadRequest(response);
            }
            if (_couponRepo.GetAsync(coupon_C_DTO.Name).GetAwaiter().GetResult() != null)
            {
                return Results.BadRequest("Coupon Name already Exists");
            }

            Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);


            await _couponRepo.CreateAsync(coupon);
            await _couponRepo.SaveAsync();
            CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

            response.Result = couponDTO;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.Created;
            return Results.Ok(response);

            //return Results.CreatedAtRoute("GetCoupon",new { id = coupon.Id }, couponDTO);
            //return Results.Created($"/api/coupon/{coupon.Id}", coupon);
        }
        //[Authorize]
        private async static Task<IResult> UpdateCoupon(ICouponRepository _couponRepo, IMapper _mapper,
                IValidator<CouponUpateDTO> _validation, [FromBody] CouponUpateDTO coupon_U_DTO)
        {
            APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

            var validationResult = await _validation.ValidateAsync(coupon_U_DTO);
            if (!validationResult.IsValid)
            {
                response.ErrorMessage.Add(validationResult.Errors.FirstOrDefault().ToString());
                return Results.BadRequest(response);
            }


            await _couponRepo.UpdateAsync(_mapper.Map<Coupon>(coupon_U_DTO));
            await _couponRepo.SaveAsync();

            response.Result = _mapper.Map<CouponDTO>(await _couponRepo.GetAsync(coupon_U_DTO.Id));
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }
        //[Authorize]
        private async static Task<IResult> DeleteCoupon(ICouponRepository _couponRepo, int id)
        {
            APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

            Coupon couponFromStore = await _couponRepo.GetAsync(id);
            if (couponFromStore != null)
            {
                await _couponRepo.RemoveAsync(couponFromStore);
                await _couponRepo.SaveAsync();
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.NoContent;
                return Results.Ok(response);
            }
            else
            {
                response.ErrorMessage.Add("InValid Id");
                return Results.BadRequest(response);
            }
        }
        private async static Task<IResult> GetAllCoupon(ICouponRepository _couponRepo, ILogger<Program> _logger)
        {
            APIResponse response = new();
            _logger.Log(LogLevel.Information, "Getting all Coupons");
            response.Result = await _couponRepo.GetAllAsync();
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }
    }
}

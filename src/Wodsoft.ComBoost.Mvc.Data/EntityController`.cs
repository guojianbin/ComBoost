﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Wodsoft.ComBoost.Data;
using Wodsoft.ComBoost.Data.Entity;
using Wodsoft.ComBoost.Data.Entity.Metadata;
using Wodsoft.ComBoost.Security;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Wodsoft.ComBoost.Mvc
{
    public class EntityController<T> : EntityController, IHaveEntityMetadata
        where T : class, IEntity, new()
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            EntityService = DomainProvider.GetService<EntityDomainService<T>>();
            Metadata = EntityDescriptor.GetMetadata<T>();
        }

        protected EntityDomainService<T> EntityService { get; private set; }

        public IEntityMetadata Metadata { get; private set; }
        
        [HttpGet]
        [EntityAuthorize]
        public virtual async Task<IActionResult> Index()
        {
            var context = CreateDomainContext();
            try
            {
                var model = await EntityService.ExecuteAsync<IDatabaseContext, IAuthenticationProvider, EntityDomainAuthorizeOption, IEntityViewModel <T>>(context, EntityService.List);
                foreach (var button in model.ViewButtons)
                    button.SetTarget(HttpContext.RequestServices);
                if (Request.Headers["accept-content"].Contains("application/json"))
                {
                    EntityJsonConverter entityConverter = new Mvc.EntityJsonConverter(EntityDomainAuthorizeOption.View, HttpContext.RequestServices.GetRequiredService<IAuthenticationProvider>().GetAuthentication());
                    return Content(JsonConvert.SerializeObject(model, entityConverter, EntityMetadataJsonConverter.Converter, PropertyMetadataJsonConverter.Converter, EntityViewModelJsonConverter.Converter), "application/json", System.Text.Encoding.UTF8);
                }
                return View(model);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, ex.Message);
            }
        }
        
        [HttpGet]
        [EntityAuthorize]
        public virtual async Task<IActionResult> Create()
        {
            var context = CreateDomainContext();
            try
            {
                var model = await EntityService.ExecuteAsync<IDatabaseContext, IAuthenticationProvider, EntityDomainAuthorizeOption, IEntityEditModel<T>>(context, EntityService.Create);
                if (Request.Headers["accept-content"].Contains("application/json"))
                {
                    EntityJsonConverter entityConverter = new Mvc.EntityJsonConverter(EntityDomainAuthorizeOption.Create, HttpContext.RequestServices.GetRequiredService<IAuthenticationProvider>().GetAuthentication());
                    return Content(JsonConvert.SerializeObject(model, entityConverter, EntityMetadataJsonConverter.Converter, PropertyMetadataJsonConverter.Converter, EntityEditModelJsonConverter.Converter), "application/json", System.Text.Encoding.UTF8);
                }
                return View("Edit", model);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, ex.Message);
            }
        }
        
        [HttpGet]
        [EntityAuthorize]
        public virtual async Task<IActionResult> Edit()
        {
            var context = CreateDomainContext();
            try
            {
                var model = await EntityService.ExecuteAsync<IDatabaseContext, IAuthenticationProvider, IValueProvider, EntityDomainAuthorizeOption, IEntityEditModel<T>>(context, EntityService.Edit);
                if (Request.Headers["accept-content"].Contains("application/json"))
                {
                    EntityJsonConverter entityConverter = new Mvc.EntityJsonConverter(EntityDomainAuthorizeOption.Edit, HttpContext.RequestServices.GetRequiredService<IAuthenticationProvider>().GetAuthentication());
                    return Content(JsonConvert.SerializeObject(model, entityConverter, EntityMetadataJsonConverter.Converter, PropertyMetadataJsonConverter.Converter, EntityEditModelJsonConverter.Converter), "application/json", System.Text.Encoding.UTF8);
                }
                return View(model);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, ex.Message);
            }
            catch (EntityNotFoundException ex)
            {
                return StatusCode(404, ex.Message);
            }
        }
        
        [HttpGet]
        [EntityAuthorize]
        public virtual async Task<IActionResult> Detail()
        {
            var context = CreateDomainContext();
            try
            {
                var model = await EntityService.ExecuteAsync<IDatabaseContext, IAuthenticationProvider, IValueProvider, EntityDomainAuthorizeOption, IEntityEditModel<T>>(context, EntityService.Detail);
                if (Request.Headers["accept-content"].Contains("application/json"))
                {
                    EntityJsonConverter entityConverter = new Mvc.EntityJsonConverter(EntityDomainAuthorizeOption.Detail, HttpContext.RequestServices.GetRequiredService<IAuthenticationProvider>().GetAuthentication());
                    return Content(JsonConvert.SerializeObject(model, entityConverter, EntityMetadataJsonConverter.Converter, PropertyMetadataJsonConverter.Converter, EntityEditModelJsonConverter.Converter), "application/json", System.Text.Encoding.UTF8);
                }
                return View(model);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, ex.Message);
            }
            catch (EntityNotFoundException ex)
            {
                return StatusCode(404, ex.Message);
            }
        }
        
        [HttpPost]
        [EntityAuthorize]
        public virtual async Task<IActionResult> Remove()
        {
            var context = CreateDomainContext();
            try
            {
                await EntityService.ExecuteAsync<IDatabaseContext, IAuthenticationProvider, IValueProvider, EntityDomainAuthorizeOption>(context, EntityService.Remove);
                return StatusCode(200);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, ex.Message);
            }
            catch (EntityNotFoundException ex)
            {
                return StatusCode(404, ex.Message);
            }
        }
        
        [HttpPost]
        [EntityAuthorize]
        public virtual async Task<IActionResult> Update()
        {
            var context = CreateDomainContext();
            try
            {
                var result = await EntityService.ExecuteAsync<IDatabaseContext, IAuthenticationProvider, IValueProvider, EntityDomainAuthorizeOption, IEntityUpdateModel<T>>(context, EntityService.Update);
                if (result.IsSuccess)
                    return StatusCode(204);
                Response.StatusCode = 400;
                return Json(result.ErrorMessage.Select(t =>
                    new
                    {
                        Property = t.Key.ClrName,
                        Name = t.Key.Name,
                        ErrorMessage = t.Value
                    }));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, ex.Message);
            }
            catch (EntityNotFoundException ex)
            {
                return StatusCode(404, ex.Message);
            }
        }
                
        [HttpGet]
        [EntityAuthorize]
        public virtual async Task<IActionResult> Selector()
        {
            var context = CreateDomainContext();
            try
            {
                var model = await EntityService.ExecuteAsync<IDatabaseContext, IAuthenticationProvider, EntityDomainAuthorizeOption, IEntityViewModel <T>>(context, EntityService.List);
                if (Request.Headers["accept-content"].Contains("application/json"))
                {
                    EntityJsonConverter entityConverter = new Mvc.EntityJsonConverter(EntityDomainAuthorizeOption.View, HttpContext.RequestServices.GetRequiredService<IAuthenticationProvider>().GetAuthentication());
                    return Content(JsonConvert.SerializeObject(model, entityConverter, EntityMetadataJsonConverter.Converter, PropertyMetadataJsonConverter.Converter, EntityViewModelJsonConverter.Converter), "application/json", System.Text.Encoding.UTF8);
                }
                return View(model);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, ex.Message);
            }
        }
        
        [HttpGet]
        [EntityAuthorize]
        public virtual async Task<IActionResult> MultipleSelector()
        {
            var context = CreateDomainContext();
            try
            {
                var model = await EntityService.ExecuteAsync<IDatabaseContext, IAuthenticationProvider, EntityDomainAuthorizeOption, IEntityViewModel<T>>(context, EntityService.List);
                if (Request.Headers["accept-content"].Contains("application/json"))
                {
                    EntityJsonConverter entityConverter = new Mvc.EntityJsonConverter(EntityDomainAuthorizeOption.View, HttpContext.RequestServices.GetRequiredService<IAuthenticationProvider>().GetAuthentication());
                    return Content(JsonConvert.SerializeObject(model, entityConverter, EntityMetadataJsonConverter.Converter, PropertyMetadataJsonConverter.Converter, EntityViewModelJsonConverter.Converter), "application/json", System.Text.Encoding.UTF8);
                }
                return View(model);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, ex.Message);
            }
        }
    }
}

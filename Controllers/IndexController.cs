using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BangDreamChartsConverter.Exceptions;
using BangDreamChartsConverter.HttpPost;
using BangDreamChartsConverter.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BangDreamChartsConverter.Controllers
{
    public class IndexController:BaseController
    {
        private IConvertService _convertService;

        public IndexController(IConvertService convertService)
        {
            _convertService = convertService;
        }

        [Route("")]
        public IActionResult Index()
        {
            ViewData["Title"] = "邦邦谱面转换器";
            return View();
        }

        [HttpPost]
        [Route("api/{controller}/{action}")]
        public AjaxResponse<string> Convert([FromBody] ConvertPost convertPost)
        {
            try
            {
                var defaultChart = _convertService.GenerateDefaultChart(convertPost.InputText, convertPost.From,
                    convertPost.CheckRepeat, convertPost.Delay);
                return new AjaxResponse<string>(_convertService.CovertDefaultChart(defaultChart, convertPost.To));
            }
            catch (FormatErrorException e)
            {
                return new AjaxResponse<string>(new ErrorInfo("谱面转换出错，请确认格式是否正确"));
            }
            catch (OutputErrorException e)
            {
                return new AjaxResponse<string>(new ErrorInfo("谱面导出时发生错误"));
            }
            catch (Exception e)
            {
                return new AjaxResponse<string>(new ErrorInfo(e.Message));
            }
        }
    }
}

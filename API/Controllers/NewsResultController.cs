﻿using System;
using EntityFramework;
using EntityFramework.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NewsResultController : ControllerBase
    {
        public NewsResultController()
        {
            
        }
        
        [EnableCors]
        [HttpPost]
        public NewsResult Post([FromBody] UrlModel url)
        {
            WebParser.Website webParser = null;
            if (url.Url.Contains("infowars"))
                webParser = new WebParser.InfoWarsParser(url.Url);
            else if(url.Url.Contains("nbcnews"))
                webParser = new WebParser.NbcParser(url.Url);

            if (webParser == null)
                return null;
            MLPrediction.MLModel1.ModelInput sampleData = new MLPrediction.MLModel1.ModelInput()
            {
                Title = @webParser.Title,
                Text = @webParser.Content,
                Subject = @webParser.Subject,
                Date = @webParser.Date.ToString(),
            };
            var predictionResult = MLPrediction.MLModel1.Predict(sampleData);
            var result = new NewsResult(){Id = Guid.NewGuid(), Decision = predictionResult.Prediction, SearchDate = webParser.Date, StatisticsId = Guid.NewGuid(), Link = url.Url};
            using var ctx = new AppDbContext();
            ctx.ResultsHistory.Add(result);
            ctx.SaveChanges();
            return result;
        }
    }
}
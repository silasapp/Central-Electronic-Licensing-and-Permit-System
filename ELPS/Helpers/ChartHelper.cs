using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace ELPS.Helpers
{
    public class ChartHelper
    {
        public Highcharts LineChart(Series[] series, List<string> category, string title, string yAxis, string chartName, string tooltip)
        {
            Highcharts chart = new Highcharts(chartName)
                    .InitChart(new Chart
                    {
                        DefaultSeriesType = ChartTypes.Line,
                        MarginRight = 130,
                        MarginBottom = 25,
                        ClassName = chartName
                    })
                    .SetTitle(new Title
                    {
                        Text = title,
                        X = -20
                    })
                    .SetSubtitle(new Subtitle
                    {
                        Text = "Source: www.elps.nuprc.gov.ng",
                        X = -20
                    })
                    .SetXAxis(new XAxis { Categories = category.ToArray() })// ChartsData.Categories
                    .SetYAxis(new YAxis
                    {
                        Title = new YAxisTitle { Text = yAxis },
                        PlotLines = new[]
                            {
                                new YAxisPlotLines
                                    {
                                        Value = 0,
                                        Width = 1,
                                        Color = ColorTranslator.FromHtml("#808080")
                                    }
                            }
                    })
                    .SetTooltip(new Tooltip
                    {
                        Formatter = tooltip
                    })
                    .SetLegend(new Legend
                    {
                        Layout = Layouts.Vertical,
                        Align = HorizontalAligns.Right,
                        VerticalAlign = VerticalAligns.Top,
                        X = -10,
                        Y = 100,
                        BorderWidth = 0
                    })
                    .SetSeries(series);
            return chart;
        }

        public Highcharts pieChart(object[] dt, string title, string name, string ChartName, string tooltip)
        {
            Highcharts chart = new Highcharts(ChartName)
              .InitChart(new Chart { PlotShadow = false, PlotBackgroundColor = null, PlotBorderWidth = null })
              .SetTitle(new Title { Text = title })
              .SetTooltip(new Tooltip { PointFormat = tooltip })
              .SetPlotOptions(new PlotOptions
              {
                  Pie = new PlotOptionsPie
                  {
                      AllowPointSelect = true,
                      Cursor = Cursors.Pointer,
                      DataLabels = new PlotOptionsPieDataLabels { Enabled = true, Color = System.Drawing.Color.White, Distance = -20, Inside = true, Format = "{y}" },
                      ShowInLegend = true
                      //Colors = chartColor

                  }
              })
              .SetSeries(new Series
              {
                  Type = ChartTypes.Pie,
                  Name = name,
                  Data = new Data(dt),

              });
            return chart;
        }

        public Highcharts DpieChart(object[] dt, string title, string name, string ChartName, System.Drawing.Color[] chartColor)
        {
            Highcharts chart = new Highcharts(ChartName)
              .InitChart(new Chart { PlotShadow = false, PlotBackgroundColor = null, PlotBorderWidth = null })
              .SetTitle(new Title { Text = title })
              .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ this.percentage.toFixed(2) +' %'; }" })
              .SetPlotOptions(new PlotOptions
              {
                  Pie = new PlotOptionsPie
                  {
                      AllowPointSelect = true,
                      Cursor = Cursors.Pointer,
                      DataLabels = new PlotOptionsPieDataLabels { Enabled = true, Color = System.Drawing.Color.White, Distance = -20, Inside = true, Format = "{y}" },
                      ShowInLegend = true,
                      Colors = chartColor

                  }
              })
              .SetSeries(new Series
              {
                  Type = ChartTypes.Pie,
                  Name = name,
                  Data = new Data(dt),

              });
            return chart;
        }

        public Highcharts BarChartWithDrillDown(List<object> points, string[] categories, string name, string title, string yAxisTitle, string subTitle)
        {

            Highcharts chart = new Highcharts("chart")
                .InitChart(new Chart { DefaultSeriesType = ChartTypes.Column })
                .SetTitle(new Title { Text = title })
                .SetSubtitle(new Subtitle { Text = subTitle })
                .SetXAxis(new XAxis { Categories = categories })
                .SetYAxis(new YAxis { Title = new YAxisTitle { Text = yAxisTitle } })
                .SetLegend(new Legend { Enabled = true })
                .SetTooltip(new Tooltip { Formatter = "TooltipFormatter" })
                .SetPlotOptions(new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        Cursor = Cursors.Pointer,
                        //Stacking = Stackings.Normal,
                        Point = new PlotOptionsColumnPoint
                        {

                            Events = new PlotOptionsColumnPointEvents { Click = "ColumnPointClick" }
                        },
                        DataLabels = new PlotOptionsColumnDataLabels
                        {
                            Enabled = true,
                            Color = Color.FromName("colors[0]"),
                            Formatter = "function() { return this.y +'Ltr'; }",
                            Style = "fontWeight: 'bold'"
                        }
                    }
                })
                .SetSeries(new Series
                {
                    Name = name,
                    Data = new Data(points.ToArray()),
                    Color = Color.FromName("colors[0]")
                })
                .SetExporting(new Exporting { Enabled = false })
                .AddJavascripFunction(
                    "TooltipFormatter",
                    @"var point = this.point, s = this.x +':<b>'+ this.y +'Ltr Sales</b><br/>';
                      if (point.drilldown) {
                        s += 'Click to view '+ point.category +' Branches';
                      } else {
                        s += 'Click to return to Dealers';
                      }
                      return s;"
                )
                .AddJavascripFunction(
                    "ColumnPointClick",
                    @"var drilldown = this.drilldown;
                      if (drilldown) { // drill down
                        setChart(drilldown.name, drilldown.categories, drilldown.data.data, drilldown.color);
                      } else { // restore
                        setChart(name, categories, data);
                      }"
                )
                .AddJavascripFunction(
                    "setChart",
                    @"chart.xAxis[0].setCategories(categories);
                      chart.series[0].remove();
                      chart.addSeries({
                         name: name,
                         data: data,
                         color: color || 'white'
                      });",
                    "name", "categories", "data", "color"
                )
                .AddJavascripVariable("colors", "Highcharts.getOptions().colors")
                .AddJavascripVariable("name", "'{0}'".FormatWith(name))
                .AddJavascripVariable("categories", JsonSerializer.Serialize(categories))
                .AddJavascripVariable("data", JsonSerializer.Serialize(points.ToArray()));
            return chart;
        }

        public Highcharts MultiBarChart(Series[] series, List<string> category, string yAxis, string title, string chartName, string tooltip, string pointToolip)
        {

            Highcharts chart = new Highcharts(chartName)
           .InitChart(new Chart { DefaultSeriesType = ChartTypes.Column })
           .SetTitle(new Title { Text = title })
           .SetSubtitle(new Subtitle { Text = "Source: www.elps.nuprc.gov.ng" })
           .SetXAxis(new XAxis { Categories = category.ToArray() })
           .SetYAxis(new YAxis
           {
               Min = 0,
               Title = new YAxisTitle { Text = yAxis }
           })
           .SetLegend(new Legend
           {
               Layout = Layouts.Horizontal,
               Align = HorizontalAligns.Center,
           })
           //.SetTooltip(new Tooltip { Formatter = @"function() { return ''+ this.x +': '+ this.y +' Ltr'; }" })
           .SetTooltip(new Tooltip
           {
               HeaderFormat = @"<span style=""font-size:10px"">{point.key}</span><table>",
               PointFormat = pointToolip,
               FooterFormat = @"</table>",
               Shared = true,
               UseHTML = true
           })
           .SetPlotOptions(new PlotOptions
           {
               Column = new PlotOptionsColumn
               {
                   PointPadding = 0.2,
                   BorderWidth = 0
               }
           })
           .SetSeries(series);
            return chart;
        }

    }
}
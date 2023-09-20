<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BehaviorTrends.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Reporting.DR.BehaviorTrends" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />
        <div class="card mt-4 px-4 pb-4" id="series_behavior_card">
            <div class="d-flex">
                <h3 class="w-100">Series Behavior</h3>
                <i class="floating-fa fas fa-info-circle mt-2" data-toggle="tooltip" data-placement="bottom" title="Data is calculated by looking for commonalities between IP Addresses and series watched. The number shows the number of IP Addresses that have interactions with both series."></i>
            </div>
            <div class="skeleton-loader">
                <div class="well py-4"></div>
                <div class="well py-4"></div>
                <div class="well py-4"></div>
                <div class="well py-4"></div>
            </div>
        </div>
        <div class="card mt-4 px-4 pb-4" id="sermon_behavior_card">
            <div class="d-flex">
                <h3 class="w-100">Item Behavior</h3>
                <i class="floating-fa fas fa-info-circle mt-2" data-toggle="tooltip" data-placement="bottom" title="Data is calculated by looking for commonalities between IP Addresses and sermons watched. The number shows the number of IP Addresses that have interactions with both sermons."></i>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>


<script>
    $(document).ready(() => {
        getBehaviorTrends()
    })
    function getBehaviorTrends() {
        const start = Date.now()
        $.ajax({
            type: "POST",
            url: window.location + "&isBehaviorTrendRequest=true",
            data: "",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (res) {
                let elapsed = Date.now() - start
                console.log('Execution time: ' + elapsed + ' ms')
                let ipData = []
                let sermonInfo = []
                for (let i = 0; i < res.IP.length; i++) {
                    ipData.push({ ip: res.IP[i][0].IpAddress, data: res.IP[i] })
                }
                for (let i = 0; i < res.Series.length; i++) {
                    sermonInfo.push({ Id: res.Series[i].EntityId, Title: res.Series[i].Title, Series: res.Series[i].Series })
                }
                for (let i = 0; i < ipData.length; i++) {
                    let sermons = ipData[i].data.map(d => { return d.EntityId }).filter((value, index, array) => array.indexOf(value) === index)
                    ipData[i].sermons = sermons.reduce((acc, v, i) =>
                        acc.concat(sermons.slice(i + 1).map(w => {
                            if (v < w) {
                                return { first: v, second: w }
                            }
                            return { first: w, second: v }
                        })),
                        [])
                    let series = ipData[i].data.map(d => { return d.Series }).filter((value, index, array) => array.indexOf(value) === index)
                    ipData[i].series = series.reduce((acc, v, i) =>
                        acc.concat(series.slice(i + 1).map(w => {
                            if (v < w) {
                                return { first: v, second: w }
                            }
                            return { first: w, second: v }
                        })),
                        [])
                }
                function reduction(p, c) {
                    let idx = -1
                    if (p && p.length > 0) {
                        p.forEach((d, dIdx) => {
                            if (d.arr.first == c.first && d.arr.second == c.second) {
                                idx = dIdx
                            }
                        })
                    } else {
                        p = []
                    }
                    if (idx == -1) {
                        p.push({ arr: c, count: 0 })
                        idx = p.length - 1
                    }
                    p[idx].count++
                    return p
                }
                let sermonData = ipData.filter(d => d.sermons.length > 0).map(d => d.sermons).flat()
                var sermonCounts = sermonData.reduce((p, c) => reduction(p, c), {}).filter(d => d.count >= 30)
                console.log(sermonData)
                console.log(sermonCounts)
                let finalSermonData = []
                sermonCounts.forEach(s => {
                    let idx = finalSermonData.map(fsd => fsd.series).indexOf(s.arr.first)
                    if (idx < 0) {
                        finalSermonData.push({ series: s.arr.first, related: [{ series: s.arr.second, count: s.count }] })
                        idx = finalSermonData.length - 1
                    }
                    let sIdx = finalSermonData[idx].related.map(r => r.series).indexOf(s.arr.second)
                    if (sIdx < 0) {
                        finalSermonData[idx].related.push({ series: s.arr.second, count: s.count })
                    }
                    idx = finalSermonData.map(fsd => fsd.series).indexOf(s.arr.second)
                    if (idx < 0) {
                        finalSermonData.push({ series: s.arr.second, related: [{ series: s.arr.first, count: s.count }] })
                        idx = finalSermonData.length - 1
                    }
                    sIdx = finalSermonData[idx].related.map(r => r.series).indexOf(s.arr.first)
                    if (sIdx < 0) {
                        finalSermonData[idx].related.push({ series: s.arr.first, count: s.count })
                    }
                })
                finalSermonData.forEach(s => {
                    s.sermon = sermonInfo[sermonInfo.map(ser => ser.Id).indexOf(s.series)].Title
                    s.series = sermonInfo[sermonInfo.map(ser => ser.Id).indexOf(s.series)].Series
                    s.related.forEach(r => {
                        r.sermon = sermonInfo[sermonInfo.map(ser => ser.Id).indexOf(r.series)].Title
                        r.series = sermonInfo[sermonInfo.map(ser => ser.Id).indexOf(r.series)].Series
                    })
                    s.related = s.related.sort((a, b) => {
                        if (a.count > b.count) {
                            return -1
                        } else if (a.count < b.count) {
                            return 1
                        }
                        return 0
                    })
                })
                let seriesData = ipData.filter(d => d.series.length > 0).map(d => d.series).flat()
                var seriesCounts = seriesData.reduce((p, c) => reduction(p, c), {}).filter(d => d.count >= 30)
                let finalSeriesData = []
                seriesCounts.forEach(s => {
                    let idx = finalSeriesData.map(fsd => fsd.series).indexOf(s.arr.first)
                    if (idx < 0) {
                        finalSeriesData.push({ series: s.arr.first, related: [{ series: s.arr.second, count: s.count }] })
                        idx = finalSeriesData.length - 1
                    }
                    let sIdx = finalSeriesData[idx].related.map(r => r.series).indexOf(s.arr.second)
                    if (sIdx < 0) {
                        finalSeriesData[idx].related.push({ series: s.arr.second, count: s.count })
                    }
                    idx = finalSeriesData.map(fsd => fsd.series).indexOf(s.arr.second)
                    if (idx < 0) {
                        finalSeriesData.push({ series: s.arr.second, related: [{ series: s.arr.first, count: s.count }] })
                        idx = finalSeriesData.length - 1
                    }
                    sIdx = finalSeriesData[idx].related.map(r => r.series).indexOf(s.arr.first)
                    if (sIdx < 0) {
                        finalSeriesData[idx].related.push({ series: s.arr.first, count: s.count })
                    }
                })
                finalSeriesData.forEach(s => {
                    s.related = s.related.sort((a, b) => {
                        if (a.count > b.count) {
                            return -1
                        } else if (a.count < b.count) {
                            return 1
                        }
                        return 0
                    })
                })

                function createNodes(array) {
                    let row = document.createElement("div")
                    row.classList.add("row")
                    row.setAttribute("data-masonry", '{"percentPosition": true }')
                    array.forEach(s => {
                        let col = document.createElement("div")
                        col.classList.add("col-xs-12")
                        col.classList.add("col-md-3")
                        let well = document.createElement("div")
                        well.classList.add("well")
                        let startText = document.createElement("i")
                        startText.innerText = "People who watched..."
                        startText.classList.add("behavior-helper-text")
                        well.append(startText)
                        let seriesLink = document.createElement("a")
                        seriesLink.classList.add("hover")
                        seriesLink.href = "/reports/dr/watch-series-views?Series=" + s.series
                        seriesLink.target = "_blank"
                        let series = document.createElement("h4")
                        series.classList.add("behavior-target-series")
                        series.classList.add("text-center")
                        if (s.sermon) {
                            series.innerText = s.sermon
                        } else {
                            series.innerText = s.series
                        }
                        seriesLink.append(series)
                        well.append(seriesLink)
                        let endText = document.createElement("div")
                        endText.innerText = "also watched"
                        endText.classList.add("behavior-helper-text")
                        endText.classList.add("font-italic")
                        endText.classList.add("text-right")
                        endText.classList.add("w-100")
                        well.append(endText)
                        s.related.forEach(r => {
                            let relatedLink = document.createElement("a")
                            relatedLink.classList.add("hover")
                            relatedLink.href = "/reports/dr/watch-series-views?Series=" + r.series
                            relatedLink.target = "_blank"
                            let related = document.createElement("div")
                            related.classList.add("related-behavior")
                            related.classList.add("text-center")
                            if (r.sermon) {
                                related.innerText = `${r.sermon} (${r.count})`
                            } else {
                                related.innerText = `${r.series} (${r.count})`
                            }
                            relatedLink.append(related)
                            well.append(relatedLink)
                        })
                        col.append(well)
                        row.append(col)
                    })
                    return row
                }
                elapsed = Date.now() - start
                console.log('Execution time: ' + elapsed + ' ms')
                $('.skeleton-loader').remove()
                document.getElementById('series_behavior_card').append(createNodes(finalSeriesData))
                if (finalSeriesData.length == 0) {
                    let div = document.createElement("div")
                    div.classList.add("col-xs-12")
                    div.classList.add("col-md-3")
                    div.classList.add("font-italic")
                    div.innerText = "No series data is available."
                    document.getElementById('series_behavior_card').append(div)
                }
                $('#series_behavior_card .row').masonry()
                document.getElementById('sermon_behavior_card').append(createNodes(finalSermonData))
                if (finalSermonData.length == 0) {
                    let div = document.createElement("div")
                    div.classList.add("col-xs-12")
                    div.classList.add("col-md-3")
                    div.classList.add("font-italic")
                    div.innerText = "No item data is available."
                    document.getElementById('sermon_behavior_card').append(div)
                }
                $('#sermon_behavior_card .row').masonry()
                elapsed = Date.now() - start
                console.log('Execution time: ' + elapsed + ' ms')
            },
            failure: function (res) { console.error(res) }
        })
    }
</script>
<style>
    .floating-fa {
        float: right;
    }
</style>

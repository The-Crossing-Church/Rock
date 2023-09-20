<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CurrentSermon.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Reporting.DR.CurrentSermon" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />
        <div class="card pb-4 px-2">
            <h2 id="hdrSermon" runat="server"></h2>
            <div class="row">
                <div class="col col-xs-8">
                    <i class="floating-fa fas fa-info-circle" data-toggle="tooltip" data-placement="bottom" title="Interactions are recorded every time a user goes to the WLR page for this sermon. Unique views are calculated by IP Address"></i>
                </div>
                <div class="col col-xs-4">
                    <i class="floating-fa fas fa-info-circle" data-toggle="tooltip" data-placement="bottom" title="Data is calculated by grouping interactions by the IP Address and seeing the watch history associated with that IP Address"></i>
                </div>
            </div>
            <div class="row">
                <div class="col col-xs-8">
                    <canvas id="current-sermon-views" style="min-height: 300px;"></canvas>
                </div>
                <div class="col col-xs-4">
                    <div style="width: 90%; text-align: center; margin: auto;">
                        <div class="funnel" id="funnel-cookie">
                            <div class="metric" style="top: 10px;"></div>
                            <i class="fas fa-cookie-bite" style="margin-left: 40px;"></i>
                        </div>
                        <div class="funnel" id="funnel-tofu">
                            <div>
                                <div class="metric"></div>
                                <div>ToFu</div>
                            </div>
                        </div>
                        <div class="funnel" id="funnel-mofu">
                            <div>
                                <div class="metric"></div>
                                <div>MoFu</div>
                            </div>
                        </div>
                        <div class="funnel" id="funnel-bofu">
                            <div>
                                <div class="metric"></div>
                                <div>BoFu</div>
                            </div>
                        </div>
                        <div class="funnel" id="funnel-superfan">
                            <div>
                                <div class="metric"></div>
                                <div>Superfan</div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

<script>
    $(document).ready(() => {
        buildCurrentSermonViews()
        buildFunnel()
    })
    function buildCurrentSermonViews() {
        $.ajax({
            type: "POST",
            url: window.location + "&isViewsRequest=true",
            data: "",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (res) {
                let data = {
                    labels: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
                    datasets: [
                        { label: 'Total Views', data: [] },
                        { label: 'Unique Views', data: [] }
                    ]
                }
                for (let i = 0; i < res.length; i++) {
                    data.datasets[0].data.push(res[i].Total)
                    data.datasets[1].data.push(res[i].Unique)
                }
                let config = {
                    type: 'line',
                    data: data,
                    options: {
                        maintainAspectRatio: false,
                    }
                };
                let ctx = document.getElementById('current-sermon-views').getContext('2d')
                let chart = new Chart(ctx, config)
            },
            failure: function (res) { console.error(res) }
        })

    }
    function buildFunnel() {
        $.ajax({
            type: "POST",
            url: window.location + "&isViewersRequest=true",
            data: "",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (res) {
                let data = [0, 0, 0, 0, 0]
                for (let i = 0; i < res.length; i++) {
                    data[res[i].Range] = res[i].ViewersInRange
                }
                $('#funnel-cookie div.metric').text(data[0])
                $('#funnel-tofu div div.metric').text(data[1])
                $('#funnel-mofu div div.metric').text(data[2])
                $('#funnel-bofu div div.metric').text(data[3])
                $('#funnel-superfan div div.metric').text(data[4])
            },
            failure: function (res) { console.error(res) }
        })
    }
</script>
<style>
    .floating-fa {
        float: right;
    }

    #funnel-cookie {
        height: 50px;
    }

    .funnel {
        border-left: 25px solid transparent;
        border-right: 25px solid transparent;
        height: 0;
        margin: auto;
    }

        .funnel > div {
            position: absolute;
            width: 50%;
            left: 25%;
            text-align: center;
        }

        .funnel div div {
            font-variant: small-caps;
        }

    .metric {
        font-size: 24px;
        font-weight: bold;
    }

    #funnel-tofu {
        border-top: 50px solid #51E5FF;
        width: 300px;
    }

        #funnel-tofu div {
            top: 50px;
        }

    #funnel-mofu {
        border-top: 50px solid #1FDDFF;
        width: 250px;
    }

        #funnel-mofu div {
            top: 100px;
        }

    #funnel-bofu {
        border-top: 50px solid #00D0F5;
        width: 200px;
    }

        #funnel-bofu div {
            top: 150px;
        }

    #funnel-superfan {
        border-top: 50px solid #00BFE0;
        width: 150px;
    }

        #funnel-superfan div {
            top: 200px;
        }
</style>

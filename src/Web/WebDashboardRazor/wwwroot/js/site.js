// PRODUCT FORECASTING

var months = ["",
    "Jan", "Feb", "Mar",
    "Apr", "May", "Jun", "Jul",
    "Aug", "Sep", "Oct",
    "Nov", "Dec"
];

function onLoadProductForecasting() {
    setResponsivePlots();
    setUpProductDescriptionTypeahead();
}

function setResponsivePlots(plotSelector = ".responsive-plot") {
    // MAKE THE PLOTS RESPONSIVE
    // https://gist.github.com/aerispaha/63bb83208e6728188a4ee701d2b25ad5
    var d3 = Plotly.d3;
    var gd3 = d3.selectAll(plotSelector);
    var nodes_to_resize = gd3[0]; //not sure why but the goods are within a nested array
    window.onresize = function () {
        for (var i = 0; i < nodes_to_resize.length; i++) {
            if (nodes_to_resize[i].attributes["width"])
                Plotly.Plots.resize(nodes_to_resize[i]);
        }
    };
}

function setUpProductDescriptionTypeahead(typeaheadSelector = "#remote .typeahead") {
    var productDescriptions = new Bloodhound({
        datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        remote: {
            url: `${apiUri.catalog}/productSetDetailsByDescription?description=%QUERY`,
            wildcard: '%QUERY'
        }
    });

    $(typeaheadSelector)
        .typeahead
        ({
            minLength: 3,
            highlight: true
        },
        {
            name: 'products',
            display: 'description',
            limit: 10,
            source: productDescriptions
        })
        .on('typeahead:selected', function (e, data) {
            $("#productName").text(data.description);
            $("#productPrice").text(data.price);
            $("#productImage").attr("src", data.pictureUri).attr("alt", data.description);
            console.log(data.id);

            getProductData(data);
        });
}

function getProductData(product) {
    productId = product.id;
    description = product.description;

    getHistory(productId)
        .done(function (history) {
            if (history.length < 4) return;
            $.when(
                getForecast(history[history.length - 2], product),
                getForecast(history[history.length - 1], product)
            ).done(function (fore1, fore2) {
                if (fore1[1] == "success" && fore2[1] == "success")
                    plotLineChart(fore1[0], fore2[0], history, description)
            });
        });
}

function getForecast(st,pr) {
    var surl = `?month=${st.month}&year=${st.year}&avg=${st.avg}&max=${st.max}&min=${st.min}&count=${st.count}&prev=${st.prev}&units=${st.units}`;
    var purl = `&price=${pr.price}&color=${pr.color||""}&size=${pr.size||""}&shape=${pr.shape||""}&agram=${pr.agram||""}&bgram=${pr.bgram||""}&ygram=${pr.ygram||""}&zgram=${pr.zgram||""}`
    return $.getJSON(`${apiUri.forecasting}/product/${st.productId}/forecast${surl}${purl}`);
}

function getHistory(productId) {    
    return $.getJSON(`${apiUri.ordering}/product/${productId}/history`);
}

function getStats(productId) {    
    return $.getJSON(`${apiUri.ordering}/product/${productId}/stats`);
}

function plotLineChart(fore1, fore2, history, description) {
    var trace_real = TraceHistory(history);

    var trace_forecast = TraceForecast(trace_real.x, nextMonth(history[history.length - 1]), trace_real.y, fore1, fore2);

    var trace_mean = TraceMean(trace_real.x.concat(trace_forecast.x), trace_real.y);

    var layout = {
        title: description + ' - sales forecasting',
    };
    Plotly.newPlot('lineChart', [trace_real, trace_forecast, trace_mean], layout);
}

function TraceHistory(historyItems) {
    var y = $.map(historyItems, function (d) { return d.units; });
    var x = $.map(historyItems, function (d) { return `${months[d.month]}/${d.year}`;; });

    return {
        x: x,
        y: y,
        mode: 'lines+markers',
        name: 'history',
        line: { shape: 'spline' },
    };
}

function TraceForecast(labels, next, values, fore1, fore2) {
    return {
        x: [labels[labels.length - 2], labels[labels.length - 1], next],
        y: [values[values.length - 2], fore1, fore2],
        mode: 'lines+markers',
        name: 'forecasting',
        line: { shape: 'spline' }
    }
}

function TraceMean(labels, values) {
    var y_mean = values.reduce((previous, current) => current += previous) / values.length;
    return {
        x: labels,
        y: Array(labels.length).fill(y_mean),
        name: 'average',
        mode: 'lines'
    }
}

function nextMonth(predictor) {
    if (predictor.month == 12)
        return `${months[1]}/${predictor.year+1}`;
    else
        return `${months[predictor.month+1]}/${predictor.year}`;
}

function onLoadCountryForecasting() {
    setResponsivePlots();
}


// COUNTRY FORECASTING

function getCountryData(country) {
    $.getJSON(`${apiUri.ordering}/country/${country}/history`)
        .done(function (history) {
            if (history.length < 4) return;
            $.when(
                getCountryForecast(history[history.length - 2]),
                getCountryForecast(history[history.length - 1])
            ).done(function (fore1, fore2) {
                if (fore1[1] == "success" && fore2[1] == "success")
                    plotLineChartCountry(fore1[0], fore2[0], history, country)
            });
    });
}

function getCountryForecast(st) {
    var url = `?month=${st.month}&year=${st.year}&avg=${st.avg}&max=${st.max}&min=${st.min}&prev=${st.prev}&count=${st.count}&units=${st.units}`;
    return $.getJSON(`${apiUri.forecasting}/country/${st.country}/forecast${url}`);
}

function plotLineChartCountry(fore1, fore2, historyItems, country) {
    var trace_real = getTraceCountryHistory(historyItems);

    var trace_forecast = getTraceCountryForecast(trace_real.x, nextMonth(historyItems[historyItems.length-1]), trace_real.y, fore1, fore2);

    var trace_mean = TraceMean(trace_real.x.concat(trace_forecast.x), trace_real.y);

    var layout = {
        title: country + ' - sales forecasting',
    };
    Plotly.newPlot('lineChart', [trace_real, trace_forecast, trace_mean], layout);
}

function getTraceCountryHistory(historyItems) {
    var y = $.map(historyItems, function (d) { return d.units; });
    var x = $.map(historyItems, function (d) { return `${months[d.month]}/${d.year}`; });

    return {
        x: x,
        y: y,
        mode: 'lines+markers',
        name: 'history',
        line: { shape: 'spline' },
    };
}

function getTraceCountryForecast(labels, next, values, fore1, fore2) {
    return {
        x: [labels[labels.length - 2], labels[labels.length - 1], next],
        y: [values[values.length - 2], fore1, fore2],
        mode: 'lines+markers',
        name: 'forecasting',
        line: { shape: 'spline' }
    };
}
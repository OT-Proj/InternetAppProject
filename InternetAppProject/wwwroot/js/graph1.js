function ShowGraph(page_url, graph_id) {

    console.log(graph_id);
    //Read the data
    $(document).ready(function () {

        // get data from server (points for the graph)
        var json_result = $.ajax({
            type: "GET",
            url: "/PurchaseEvents/" + page_url,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
        });

        // draw the graph on the screen
        json_result.done(
            // Now I can use this dataset:

            // converts date from the server into presentable format
            function (lineData) {
                var parser;

                lineData.forEach(function (d) {
                    console.log(d.date);
                    if (d.date.includes("/")) {
                        parser = d3.timeParse("%Y/%m/%d");
                    }
                    else {
                        parser = d3.timeParse("%Y-%m-%d");
                    }
                    d.date_fixed = new Date(parser(d.date));
                    console.log(d.date_fixed);
                })
                //********

                // define graph properties (size, axis scale, etc.)
                var height = 200;
                var width = 650;
                var hEach = 40;

                var margin = { top: 20, right: 35, bottom: 25, left: 100 };

                width = width - margin.left - margin.right;
                height = height - margin.top - margin.bottom;

                // add graph component into the html document
                var svg = d3.select('.svg' + graph_id).append("svg")
                    .attr("width", width + margin.left + margin.right)
                    .attr("height", height + margin.top + margin.bottom)
                    .append("g")
                    .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

                // set the ranges for both axes
                var x = d3.scaleTime().range([0, width]);

                // x is dates, ranges between earliest to latest date
                x.domain(d3.extent(lineData, function (d) { return d.date_fixed; }));


                var y = d3.scaleLinear().range([height, 0]);

                // y is the depanding variable, sclaes from 0 to the maximum of the recieved data
                y.domain([0, d3.max(lineData, function (d) { return d.value; })]);

                // put data points on the screen
                var valueline = d3.line()
                    .x(function (d) { return x(d.date_fixed); })
                    .y(function (d) { return y(d.value); })
                    .curve(d3.curveMonotoneX);

                // add the line wetween the points
                svg.append("path")
                    .data([lineData])
                    .attr("class", "line")
                    .attr("d", valueline);

                //var xAxis_woy = d3.axisBottom(x).tickFormat(d3.timeFormat("Day %V"));
                var xAxis_woy = d3.axisBottom(x).tickFormat(d3.timeFormat("%Y-%m-%d")).tickValues(lineData.map(d => d.date_fixed));

                svg.append("g")
                    .attr("class", "x axis")
                    .attr("transform", "translate(0," + height + ")")
                    .call(xAxis_woy);

                //  Add the Y Axis
                //  svg.append("g").call(d3.axisLeft(y));

                svg.selectAll(".dot")
                    .data(lineData)
                    .enter()
                    .append("circle") // Uses the enter().append() method
                    .attr("class", "dot") // Assign a class for styling
                    .attr("cx", function (d) { return x(d.date_fixed) })
                    .attr("cy", function (d) { return y(d.value) })
                    .attr("r", 5);

                // draw point text next the point
                svg.selectAll(".text")
                    .data(lineData)
                    .enter()
                    .append("text") // Uses the enter().append() method
                    .attr("class", "label") // Assign a class for styling
                    .attr("x", function (d, i) { return x(d.date_fixed) })
                    .attr("y", function (d) { return y(d.value) })
                    .attr("dy", "-5")
                    .text(function (d) { return d.value; });

                svg.append('text')
                    .attr('x', -30)
                    .attr('y', -5)
                    .text('Profits per Day');



            });
    });
 
}

ShowGraph("ByDayJson", "1");
ShowGraph("CountByDayJson", "2");
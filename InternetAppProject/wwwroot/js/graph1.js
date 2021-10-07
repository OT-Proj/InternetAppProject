//Read the data
$(document).ready(function () {

    var json_result = $.ajax({
        type: "GET",
        url: "/PurchaseEvents/ByDayJson",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
    });

    json_result.done(
        // Now I can use this dataset:
        function (lineData) {

            var parser = d3.timeParse("%Y-%m-%d")

            lineData.forEach(function (d) {
                console.log(d.date);
                d.date_fixed = new Date(parser(d.date));
                console.log(d.date_fixed);
            })
            //********


            var height = 200;
            var width = 650;
            var hEach = 40;

            var margin = { top: 20, right: 35, bottom: 25, left: 100 };

            width = width - margin.left - margin.right;
            height = height - margin.top - margin.bottom;

            var svg = d3.select('body').append("svg")
                .attr("width", width + margin.left + margin.right)
                .attr("height", height + margin.top + margin.bottom)
                .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

            // set the ranges
            var x = d3.scaleTime().range([0, width]);

            x.domain(d3.extent(lineData, function (d) { return d.date_fixed; }));


            var y = d3.scaleLinear().range([height, 0]);


            y.domain([0, d3.max(lineData, function (d) { return d.value; })]);

            var valueline = d3.line()
                .x(function (d) { return x(d.date_fixed); })
                .y(function (d) { return y(d.value); })
                .curve(d3.curveMonotoneX);

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
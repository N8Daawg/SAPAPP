// Code for a variety of interaction models. Used in interaction.html, but split out from
// that file so they can be tested in isolation.
//
function downV3(event, g, context) {
  context.initializeMouseDown(event, g, context);
  if (event.altKey || event.shiftKey) {
    Dygraph.startZoom(event, g, context);
  } else {
    Dygraph.startPan(event, g, context);
  }
}

function moveV3(event, g, context) {
  if (context.isPanning) {
    Dygraph.movePan(event, g, context);
  } else if (context.isZooming) {
    Dygraph.moveZoom(event, g, context);
  }
}

function upV3(event, g, context) {
  if (context.isPanning) {
    Dygraph.endPan(event, g, context);
  } else if (context.isZooming) {
    Dygraph.endZoom(event, g, context);
  }
}

// Take the offset of a mouse event on the dygraph canvas and
// convert it to a pair of percentages from the bottom left.
// (Not top left, bottom is where the lower value is.)
function offsetToPercentage(g, offsetX, offsetY) {
  // This is calculating the pixel offset of the leftmost date.
  var xOffset = g.toDomCoords(g.xAxisRange()[0], null)[0];
  var yar0 = g.yAxisRange(0);

  // This is calculating the pixel of the higest value. (Top pixel)
  var yOffset = g.toDomCoords(null, yar0[1])[1];

  // x y w and h are relative to the corner of the drawing area,
  // so that the upper corner of the drawing area is (0, 0).
  var x = offsetX - xOffset;
  var y = offsetY - yOffset;

  // This is computing the rightmost pixel, effectively defining the
  // width.
  var w = g.toDomCoords(g.xAxisRange()[1], null)[0] - xOffset;

  // This is computing the lowest pixel, effectively defining the height.
  var h = g.toDomCoords(null, yar0[0])[1] - yOffset;

  // Percentage from the left.
  var xPct = w == 0 ? 0 : (x / w);
  // Percentage from the top.
  var yPct = h == 0 ? 0 : (y / h);

  // The (1-) part below changes it from "% distance down from the top"
  // to "% distance up from the bottom".
  return [xPct, (1-yPct)];
}

function dblClickV3(event, g, context) {
  // Reducing by 20% makes it 80% the original size, which means
  // to restore to original size it must grow by 25%

  if (!(event.offsetX && event.offsetY)){
    event.offsetX = event.layerX - event.target.offsetLeft;
    event.offsetY = event.layerY - event.target.offsetTop;
  }

  var percentages = offsetToPercentage(g, event.offsetX, event.offsetY);
  var xPct = percentages[0];
  var yPct = percentages[1];

  if (event.ctrlKey) {
    zoom(g, -.25, xPct, yPct);
  } else {
    zoom(g, +.2, xPct, yPct);
  }
}

var lastClickedGraph = null;

function clickV3(event, g, context) {
  lastClickedGraph = g;
  event.preventDefault();
  event.stopPropagation();
}

function scrollV3(event, g, context) {
  if (lastClickedGraph != g) {
    return;
  }
  var normal = event.detail ? event.detail * -1 : event.wheelDelta / 40;
  // For me the normalized value shows 0.075 for one click. If I took
  // that verbatim, it would be a 7.5%.
  var percentage = normal / 50;

  if (!(event.offsetX && event.offsetY)){
    event.offsetX = event.layerX - event.target.offsetLeft;
    event.offsetY = event.layerY - event.target.offsetTop;
  }

  var percentages = offsetToPercentage(g, event.offsetX, event.offsetY);
  var xPct = percentages[0];
  var yPct = percentages[1];

  zoom(g, percentage, xPct, yPct);
  event.preventDefault();
  event.stopPropagation();
}

// Adjusts [x, y] toward each other by zoomInPercentage%
// Split it so the left/bottom axis gets xBias/yBias of that change and
// tight/top gets (1-xBias)/(1-yBias) of that change.
//
// If a bias is missing it splits it down the middle.
function zoom(g, zoomInPercentage, xBias, yBias) {
  xBias = xBias || 0.5;
  yBias = yBias || 0.5;
  function adjustAxis(axis, zoomInPercentage, bias) {
    var delta = axis[1] - axis[0];
    var increment = delta * zoomInPercentage;
    var foo = [increment * bias, increment * (1-bias)];
    return [ axis[0] + foo[0], axis[1] - foo[1] ];
  }
  var yAxes = g.yAxisRanges();
  var newYAxes = [];
  for (var i = 0; i < yAxes.length; i++) {
    newYAxes[i] = adjustAxis(yAxes[i], zoomInPercentage, yBias);
  }

  g.updateOptions({
    dateWindow: adjustAxis(g.xAxisRange(), zoomInPercentage, xBias),
    valueRange: newYAxes[0]
    });
}

var v4Active = false;
var v4Canvas = null;

function downV4(event, g, context) {
  context.initializeMouseDown(event, g, context);
  v4Active = true;
  moveV4(event, g, context); // in case the mouse went down on a data point.
}

var processed = [];

function moveV4(event, g, context) {
  var RANGE = 7;

  if (v4Active) {
    var graphPos = Dygraph.findPos(g.graphDiv);
    var canvasx = Dygraph.pageX(event) - graphPos.x;
    var canvasy = Dygraph.pageY(event) - graphPos.y;

    var rows = g.numRows();
    // Row layout:
    // [date, [val1, stdev1], [val2, stdev2]]
    for (var row = 0; row < rows; row++) {
      var date = g.getValue(row, 0);
      var x = g.toDomCoords(date, null)[0];
      var diff = Math.abs(canvasx - x);
      if (diff < RANGE) {
        for (var col = 1; col < 3; col++) {
          // TODO(konigsberg): these will throw exceptions as data is removed.
          var vals =  g.getValue(row, col);
          if (vals == null) { continue; }
          var val = vals[0];
          var y = g.toDomCoords(null, val)[1];
          var diff2 = Math.abs(canvasy - y);
          if (diff2 < RANGE) {
            var found = false;
            for (var i in processed) {
              var stored = processed[i];
              if(stored[0] == row && stored[1] == col) {
                found = true;
                break;
              }
            }
            if (!found) {
              processed.push([row, col]);
              drawV4(x, y);
            }
            return;
          }
        }
      }
    }
  }
}

function upV4(event, g, context) {
  if (v4Active) {
    v4Active = false;
  }
}

function dblClickV4(event, g, context) {
  restorePositioning(g);
}

function drawV4(x, y) {
  var ctx = v4Canvas;

  ctx.strokeStyle = "#000000";
  ctx.fillStyle = "#FFFF00";
  ctx.beginPath();
  ctx.arc(x,y,5,0,Math.PI*2,true);
  ctx.closePath();
  ctx.stroke();
  ctx.fill();
}

function captureCanvas(canvas, area, g) {
  v4Canvas = canvas;
}

function restorePositioning(g) {
  g.updateOptions({
    dateWindow: null,
    valueRange: null
  });
}

//
// APIs Added by TI for graphing widget
//


// Zoom in and out using mouse scroll wheel
function zoomMouseScroll(event, g, context) {

    // Capture the graph range limits
    var xLeftLimit = g.xAxisExtremes()[0];
    var xRightLimit = g.xAxisExtremes()[1];

    // X Offset of the scroll points
    xScrollPointDom = event.offsetX;

    // Left most point on the graphs
    xLeftDom = g.toDomCoords(g.xAxisRange()[0])[0];
    xRightDom = g.toDomCoords(g.xAxisRange()[1])[0];
    widthDom = xRightDom - xLeftDom;

    // xPoint percentage offset within graph space
    xScrollPointPercent = (xScrollPointDom - xLeftDom) / widthDom;

    // Scroll percentage as indicated by the wheel event polarity
    // If wheelDelta is positive, zoom in
    // If wheelDelta is negative, zoom out
    mouseScrollPercent = (event.wheelDelta >= 0) ? 0.20 : -0.20;

    // New left and right extremes
    deltaRange = g.xAxisRange()[1] - g.xAxisRange()[0];
    xLeftNew = g.xAxisRange()[0] + (xScrollPointPercent * mouseScrollPercent * deltaRange);
    xRightNew = g.xAxisRange()[1] - ((1- xScrollPointPercent) * mouseScrollPercent * deltaRange);

    // If the left x value is greater than the right x value
    // then don't do anything else the x axis will be reverted
    if (xRightNew <= xLeftNew)
    {
        return;
    }
    if (xLeftNew < xLeftLimit)
    {
        xLeftNew = xLeftLimit;
    }
    if (xRightNew > xRightLimit)
    {
        xRightNew = xRightLimit;
    }

    g.updateOptions({
      dateWindow: [xLeftNew, xRightNew],
      });

    event.preventDefault();
    event.stopPropagation();

}

// Restore graph to full range on double click
function dblClickRestoreZoom(event, g, context) {
    // Restore the graph range limits
    var xLeftLimit = g.xAxisExtremes()[0];
    var xRightLimit = g.xAxisExtremes()[1];
    g.updateOptions({
      dateWindow: [xLeftLimit, xRightLimit],
      });
    event.preventDefault();
    event.stopPropagation();
}

// Add a marker on mouse click with shift key pressed
function shiftClickAddMarker(event, g, context) {
    if (event.shiftKey)
    {
        var e = {dygraph: g, canvasx: event.x};
        var callback_plugin_pairs=g.eventListeners_["click"];

        if(callback_plugin_pairs)
        {
            for(var i=callback_plugin_pairs.length - 1;i >= 0;i--)
            {
                var plugin=callback_plugin_pairs[i][0];
                var callback=callback_plugin_pairs[i][1];
                callback.call(plugin,e);
            }
        }
    }
}

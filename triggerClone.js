(function(){

/*jslint node: true, browser: true */
/*global $tw: false */
"use strict";

var Widget = require("$:/core/modules/widgets/widget.js").widget;

// Extend the Widget.prototype object to create a new widget
var HttpRequestWidget = function(parseTreeNode,options) {
  this.initialise(parseTreeNode,options);
};

// Inherit from the Widget.prototype object
HttpRequestWidget.prototype = new Widget();

// Override the render method to add the button element
HttpRequestWidget.prototype.render = function(parent,nextSibling) {
  this.parentDomNode = parent;
  var button = this.document.createElement("button");
  button.innerHTML = "Click Me";
  parent.insertBefore(button,nextSibling);
  this.domNodes.push(button);
  this.computeAttributes();
  this.execute();
};

// Override the execute method to listen to events
HttpRequestWidget.prototype.execute = function() {
  var button = this.domNodes[0];
  if (!button) {
    return;
  }
  button.onclick = function() {
    fetch(<<URL-of-the-functionApp-to-clone-the-updates>>,  {
      method: 'GET',
      headers: {
        'x-functions-key': <<x-functions-key-string>>
      }
    })
      .then(response => response.json())
      .then(data => console.log(data))
      .catch(error => console.error(error));
  };
};

exports.triggerClone= HttpRequestWidget;

})();

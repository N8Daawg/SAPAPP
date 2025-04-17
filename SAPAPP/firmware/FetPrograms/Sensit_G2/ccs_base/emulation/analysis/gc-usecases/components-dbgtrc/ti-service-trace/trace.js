/* eslint "no-console" : "off" */
/* global Q:false */
var TITraceAgent = TITraceAgent || {};
(function (TITraceAgent) {
  function getCloudTracePort()
  {
    var xhttp = new XMLHttpRequest();
    var portDefer = Q.defer();

    xhttp.onreadystatechange = function() {
      if (this.readyState == 4) {
        if (this.status == 200) {
          portDefer.resolve(this.responseText);
        } else {
          portDefer.reject("request for CloudTracePort failed");
        }
      }
    }
    xhttp.open("GET","http://"+ location.host + "/dbgtrc/cloudtraceport");
    xhttp.send();
    return portDefer.promise;
  }
  function createClientModule(host, port /*, subProtocol*/) {
    var eventListeners = {};
    var moduleObj = {
      addListener: function (type, listener) {
        if (!eventListeners[type]) {
          eventListeners[type] = [];
        }
        eventListeners[type].push(listener);
      },
      removeListener: function (type, listener) {
        if (eventListeners[type]) {
          var typeListeners = eventListeners[type];
          for (var i = 0; i < typeListeners.length; i++) {
            if (typeListeners[i] === listener) {
              typeListeners[i] = null;
              break;
            }
          }
        }
      },
      getSubModule: null,
      close: null,
    };
    var internalModuleObj = {
      createSubModule: null,
      listCommands: null,
    };
    function createCommandDispatch() {
      var commandID = 1; // start from 1, since 0 is eqiv to null
      var pendingCommands = {};
      var rejectMsg = { message: "Module Closed" };
      var commandDispatchObj = {
        exec: function (ws, commandName, data) {
          var defCommand = Q.defer();
          var obj = {
            command: commandName,
            id: commandID++,
            data: data,
          };
          pendingCommands[obj.id] = defCommand;
          var message = JSON.stringify(obj);
          console.log("sent command " + obj.id.toString() + ": " + commandName);
          try {
            ws.send(message);
          }
          catch (e) {
            console.log("command "+obj.id.toString() + " rejected");
            console.log(e)
            defCommand.reject(rejectMsg);
          }
          return defCommand.promise;
        },
        ret: function (retObj) {
          var response = retObj.response;
          var error = retObj.error;
          var data = retObj.data;
          var id = response || error;
          var defCommand = pendingCommands[id];
          if (defCommand) {
            if (response) {
              defCommand.resolve(data);
            } else {
              console.log("Error response: "+data.message);
              defCommand.reject(data);
            }
            delete pendingCommands[id];
            console.log("received response for cmd "+id.toString());
          } else {
            console.error("commandDispatch : ret , Error, no promise found corresponding to id : " + id);
          }
        },
      };
      function cleanUp() {
        for (var key in pendingCommands) {
          if (pendingCommands.hasOwnProperty(key)) {
            var commandPromise = pendingCommands[key];
            commandPromise.reject(rejectMsg);
          }
        }
        pendingCommands = {};
        commandDispatchObj.exec = function () {
          var defCommand = Q.defer();
          defCommand.reject(rejectMsg);
          return defCommand.promise;
        };
        commandDispatchObj.ret = function() {
          // do nothing
          return;
        };
      }
      moduleObj.addListener("close", cleanUp);
      return commandDispatchObj;
    }
    var commandDispatch = createCommandDispatch();
    var eventDispatch = {
      dispatch: function (listeners, retObj) {
        var typeListeners = listeners[retObj.event];
        if (typeListeners) {
           for (var _i = 0, typeListeners_1 = typeListeners; 
                _i < typeListeners_1.length;
                _i++) {
             var listener = typeListeners_1[_i];
             if (listener) {
               listener(retObj.data);
             }
           } 
        }
      },
    };
    function socketURL() {
      return "ws://" + host + ":" + port; //127.0.0.1:"+port;
    }
    var subModulePromises = {};
    moduleObj.getSubModule = function(subModuleName) {
      var subModulePromise = subModulePromises[subModuleName];
      if (!subModulePromise) {
        subModulePromise = internalModuleObj.createSubModule(subModuleName)
        .then(function (data) {
          return createClientModule(host, data.port, data.subProtocol);
        }).then(function (subModule) {
          function cleanUp() {
            subModulePromises[subModuleName] = null;
          }
          subModule.addListener("close", cleanUp);
          return subModule;
        }).catch(function (err) {
          subModulePromises[subModuleName] = null;
          throw err;
        });
        subModulePromises[subModuleName] = subModulePromise;
      }
      return subModulePromise;
    };

    function createCommand(ws, fullCommandName) {
      var commandNameParts = fullCommandName.split(".");
      var parentObj = moduleObj;
      var parentNamespace = "";
      function createAddListener(eventTypePrefix) {
        return function (type, listener) {
          type = eventTypePrefix+type;
          moduleObj.addListener(type, listener);
        };
      }
      function createRemoveListener(eventTypePrefix) {
        return function (type, listener) {
          type = eventTypePrefix + type;
          moduleObj.removeListener(type, listener);
        };
      }
      for (var i = 0; i < commandNameParts.length - 1; i++) {
        var currentNamespacePart = commandNameParts[i];
        parentNamespace += commandNameParts[i];
        var newObj = parentObj[currentNamespacePart];
        if (!newObj) {
          var eventTypePrefix = parentNamespace + ".";
          newObj = {
            addListener: createAddListener(eventTypePrefix),
            removeListener: createRemoveListener(eventTypePrefix),
          };
        }
        parentObj[currentNamespacePart] = newObj;
        parentObj = newObj;
        parentNamespace = parentNamespace + ".";
      }
      var commandName = commandNameParts[commandNameParts.length-1];
      if ("createSubModule" === fullCommandName ||
          "listCommands" === fullCommandName) {
        parentObj = internalModuleObj;
      }
      function theCommand() {
        var data = Array.prototype.slice.call(arguments);
        return commandDispatch.exec(ws, fullCommandName, data);
      }
      parentObj[commandName] = theCommand;
    }
    var setUpDef = Q.defer();
    var ws = new WebSocket(socketURL());
    function pageUnloadHandler() {
      ws.close();
    }
    ws.onclose = function() {
      setUpDef.reject("socket closed");
      eventDispatch.dispatch(eventListeners, {
        event: "close",
        data: {
          message: "socket closed",
        },
      });
      window.removeEventListener("unload", pageUnloadHandler);
    };
    ws.onerror = function() {
      setUpDef.reject("socket error");
      eventDispatch.dispatch(eventListeners, {
        event: "error",
        data: {
          message: "socket error",
        }
      });
    };
    ws.onopen = function() {
      try {
        window.addEventListener("unload", pageUnloadHandler);
        ws.onmessage = function (msgEvt) {
          var retObj = JSON.parse(msgEvt.data);
          if (retObj.event) {
            eventDispatch.dispatch(eventListeners, retObj);
          } else {
            commandDispatch.ret(retObj);
          }
        };
        moduleObj.close = function() {
          ws.close();
          return Q();
        };
        createCommand(ws, "listCommands");
        internalModuleObj.listCommands()
        .then(function (dataObj) {
          for (var _i = 0, _a = dataObj.commands; _i < _a.length; _i++) {
            var command = _a[_i];
            if (command !== "listCommands") {
              createCommand(ws, command);
            }
          }
          setUpDef.resolve(moduleObj);
        }).catch(function (err) {return setUpDef.reject(err);});
      }
      catch (err) {
        setUpDef.reject(err);
      }
    };
    return setUpDef.promise;
  }
  var mod;
  function Init() {
    if (!mod) {
      mod = Q.defer();

      getCloudTracePort().then(function (port) {
	createClientModule("127.0.0.1", port).then(function (agent) {
	  mod.resolve(agent);
	}).catch(function (err) {
	  createClientModule("localhost", port).then(function (agent) {
            mod.resolve(agent);
	  }).catch(function (err) {
            mod.reject(err);
	  });
        });
      }).catch(function (err) {
        mod.reject(err);
      });
    }
    return mod.promise;
  }
  TITraceAgent.Init = Init;
})(TITraceAgent);

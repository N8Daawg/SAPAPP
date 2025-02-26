/***************************************************************************************************
 * Copyright (c) 2016-2018 Texas Instruments and others All rights reserved. This program and the
 * accompanying materials are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors: Brian Cruickshank - Initial API and implementation
 **************************************************************************************************/
var gc = gc || {};
gc.desktop = gc.desktop || {};

	if (window.parent != window)
	{
	    window.parent.gc = window.parent.gc || {};
	    window.parent.gc.app = gc;
	}

	if (window.parent.gc)
	{
	    // take the designer from the parent iframe, if available.
	    gc.designer = gc.designer || window.parent.gc.designer;
	}
	if (window.global && global.document && global.document.gc)
	{
	    // take the designer from the global node-webkit document if available
	    gc.designer = gc.designer || global.document.gc.designer;
	}



if (gc.desktop.isDesktop === undefined)
{
    (function() // closure for private static methods and data.
    {

        function _shutdownServer() {
            var xmlhttp = new XMLHttpRequest();
            xmlhttp.onreadystatechange = function () {};
            xmlhttp.open("GET", "/api/shutdown", true);
            xmlhttp.send();
        }

        gc.desktop.isNodeWebkit = function(){
            return (typeof nw !== "undefined");
        }


        gc.desktop.isDesktop = function (is_gcDesigner) {
            /* when the files are served by a webserver, serverConfig is initialized by the server-config.js file */
            var desktop = false;
            var keepAlive = false;
            if (gc.serverConfig != null) {
                desktop = !gc.serverConfig.isOnline;
                keepAlive = gc.serverConfig.keepAliveMonitorEnabled;
            }

            /* if we are running in the desktop mode and not a CCS mode, keep the server running */
            if (gc.desktop.__serverPingHandler == null && desktop && keepAlive) {
                gc.desktop.__serverPingHandler = setInterval(function() {
                    var xmlhttp = new XMLHttpRequest();
                    xmlhttp.onreadystatechange = function () {};
                    xmlhttp.open("GET", "/api/keepalive", true);
                    xmlhttp.send();
                }, 30000);
            }

            /* if running in NW enviornment, add an NW close handle and shutdown the server */
            if (typeof nw !== "undefined" && !!window.$ti_nw_main_window) {
                 if (!gc.desktop.shutdownHdlr) {
                    gc.desktop.shutdownHdlr = function() {
                        _shutdownServer();
                        this.close(true);
                    };
                    nw.Window.get().on("close", gc.desktop.shutdownHdlr);
                }
            }

            return desktop;
        };

        gc.desktop.isMobileDevice = function () {
            var result = false;
            if ((navigator.app) || (navigator.device)) {
                result = true;
            }

            return result;
        };

        gc.desktop.isCCS = function() {
            var hostname = window.location.hostname;
            return !gc.desktop.isDesktop() && (hostname === 'localhost' || hostname === '127.0.0.1');
        };

        gc.desktop.openBrowserWindow = function(url) {
            if (!gc.desktop.isNodeWebkit()) {
                window.open(url);
            } else {
                //get absolute URL when running in desktop local server. 
                if (url.indexOf("http") != 0){
                    var segments = window.location.toString().split("/");
                    segments.pop();
                    segments.push(url);
                    url = segments.join("/");
                }
                nw.Window.open (url, {
                    frame: true,
                    min_width: 1024,
                    min_height: 800
                });
            }
        };

        gc.desktop.closeWindow = function(win,quitApp){
            var _win = win || window;
            
            // shutdown the server
            if (quitApp && gc.serverConfig.keepAliveMonitorEnabled) {
                _shutdownServer();
                window.location = "about:blank";
                _win.close();
            } else {
                _win.close();
            }

        };
        gc.desktop.minimizeWindow = function(win){
            if (gc.desktop.isNodeWebkit()){
                var _win = nw.Window.get();
                _win.minimize()
            }
        };
        gc.desktop.maximizeWindow = function(win){
            if (gc.desktop.isNodeWebkit()){
                var _win = nw.Window.get();
                _win.maximize()
            }
        };
        gc.desktop.restoreWindow = function(win){
            if (gc.desktop.isNodeWebkit()){
                var _win = nw.Window.get();
                _win.restore();

            }
        };
        gc.desktop.getPathToDesignerFolder = function(){
            return gc.designer.folderName;
        };
        gc.desktop.getPathToWorkspaceFolder = function(){
            return gc.designer.workspace.folderName;
        };
        gc.desktop.isAppPreview = function(){
            var result = false;
            var url = document.baseURI;
            if (gc.desktop.isDesktop()){
                if (url.indexOf('/workspace/') > 0){
                    result = true;
                }
            } else {
                if (url.indexOf('/preview/') > 0){
                    result = true;
                }
            }
            return result;
        };
        gc.desktop.getPathToProjectFolder = function(pathRelativeToWorkspaceFolder){
            return gc.designer.project.folderName;
        };
        gc.desktop.getPathToComponentsFolder = function(){
            return gc.designer != null ? "designer/components" : "dbgtrc/components";
        };
        gc.desktop.openExplorer = function(folderPath) {
            var xmlhttp = new XMLHttpRequest();
            xmlhttp.onreadystatechange = function () {};
            xmlhttp.open("GET", "/api/openExplorer?path=" + folderPath, true);
            xmlhttp.send();
        };
        gc.desktop.getOS = function(){
            var os  = 'linux';
            if (navigator.appVersion.indexOf("Mac") != -1) {
                os = 'osx';
            } else if (navigator.appVersion.indexOf("Win")!= -1) {
                os = 'win';
            }
            return os;
        };
        gc.desktop.getPathToSupportedDevicesJson = function() {
            var jsonFilePath = "/dbgtrc/components/ti-core-backplane/supported_devices.json";
            return jsonFilePath;
        };
        gc.desktop.getPathToTargetSetupJson = function() {
            var jsonFilePath = "/dbgtrc/components/ti-core-backplane/target_setup.json";
            return jsonFilePath;
        };

    }());
}

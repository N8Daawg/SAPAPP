/*****************************************************************
 * Copyright (c) 2018 Texas Instruments and others
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 *****************************************************************/
var gc = gc || {};
gc.databind = gc.databind || {};

(function() // closure for private static methods and data.
{
	gc.databind.TraceModel = function() 
	{
        gc.databind.AbstractPollingDataModel.call(this);
        this.init();
	};
	
	gc.databind.TraceModel.prototype = new gc.databind.AbstractPollingDataModel('tm');
	
    gc.databind.TraceModel.prototype.init = function()
    {
        gc.databind.AbstractPollingDataModel.prototype.init.call(this);
        
        //var binding = this._modelBindings.$target_connected;
        var refreshBinding = this._modelBindings.$refresh_interval;
        var activeDebugContext = new gc.databind.VariableBindValue('');
        this._modelBindings.$active_context_name = activeDebugContext; 
        activeDebugContext.addChangedListener(
        {
            onValueChanged: function() 
            {
                //...[ clear out critical errors on every context change
                var tm = gc.databind.registry.getModel('tm');
                if (tm && tm.getAllBindings)
                {
                    var bindings = tm.getAllBindings();
                    for(var bindName in bindings)
                    {
                        if (bindings.hasOwnProperty(bindName))
                        {
                            var bind = bindings[bindName];
                            if (bind.reportCriticalError)
                            {
                                bind.reportCriticalError(null);
                            }
                        }
                    }
                }
                // ...]

                refreshBinding.onRefresh();
            } 
        });
        
        gc.databind.AbstractAsyncBindValue.addQualifiersToModel(this);
    };
    
    /*
	gc.databind.TraceModel.prototype.createNewBind = function(uri)
	{
		return undefined ;//new gc.databind.internal.tm.DSEvalBind(uri, this._modelBindings.$refresh_interval);
	};*/
	
	// register program model with bindingRegistry
	gc.databind.registry.registerModel(new gc.databind.TraceModel(), true);
}());
local SCRIPT_PATH = arg[1]
local GLOBAL_STUBS_PATH = arg[2]

dofile(GLOBAL_STUBS_PATH)

local globalsAddedByScript = {}

local mt = getmetatable(_G) or {}
mt.__newindex = function (t, k, v)
   globalsAddedByScript[k] = v
   rawset(t, k, v)
end
setmetatable(_G, mt)

dofile(SCRIPT_PATH)

for k,v in pairs(globalsAddedByScript) do
   io.stdout:write(k .. "\n")
end


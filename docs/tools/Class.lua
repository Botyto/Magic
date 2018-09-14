--Main tables
FinalClasses = { }
ClassDefs = { }

Class = { }
setmetatable(Class, {
	__newindex = function(t, name, def)
		assert(rawget(ClassDefs, name) == nil, string.format("Class '%s' already defined.", name))
		ClassDefs[name] = def
		rawset(_G, name, def)
	end,
})

--Utilty functions
function IsKindOf(obj, class)
	return type(obj) == "table" and obj.__ancestors and obj.__ancestors[class]
end

--Building classes
local NonInheritedMembers = {
	__inherit = true,
	__ancestors = true,
}

local function BuildSingleClass(name)
	--get class definition
	local classdef = ClassDefs[name]
	--if definition is not found - then the class has already been built
	if not classdef then
		return FinalClasses[name]
	end
	--mark this class as built
	ClassDefs[name] = nil

	--find the actual class table
	local class = FinalClasses[name] or { }
	FinalClasses[name] = class
	rawset(_G, name, class)
	--if there was an old table of this class - clear all it's members first'
	setmetatable(class, nil)
	for member in pairs(class) do
		class[member] = nil
	end

	--define builtin members
	class["class"] = name --class name
	local HasMember = function(self, member) --has member check
		return rawget(class, member) ~= nil
	end
	class["HasMember"] = HasMember --has member check
	class["IsKindOf"] = IsKindOf --is kind of check
	class["__ctor"] = classdef.__ctor or function(self) end
	class["new"] = function(class, ...) --new object method
		local obj = { }
		setmetatable(obj, class)
		class.__ctor(obj, ...)
		return obj
	end
	class["__index"] = class.__index or function(obj, member)
		if not HasMember(obj, member) then
			error(string.format("Writing undefined member '%s.%s'", name, member))
		end
		return rawget(class, member)
	end
	class["__newindex"] = class.__newindex or function(obj, member, value) --newindex metamethod
		if not HasMember(obj, member) then
			error(string.format("Writing undefined member '%s.%s'", name, member))
		end
		rawset(obj, member, value)
	end

	--build parent class
	class["__inherit"] = classdef.__inherit or false
	if class.__inherit then
		BuildSingleClass(class.__inherit)
	end

	--fill in the ancestors list
	class["__ancestors"] = { [name] = true }
	local next_ancestor = class.__inherit
	while next_ancestor do
		if not class.__ancestors[next_ancestor] then
			class.__ancestors[next_ancestor] = true
			for k,v in pairs(FinalClasses[next_ancestor]) do
				if not rawget(class, k) then
					rawset(class, k, v)
				end
			end
			next_ancestor = FinalClasses[next_ancestor].__inherit
		elseif next_ancestor == name then
			error(string.format("Cyclic inheritance in class '%s'", name))
		end
	end

	--write all classdef fields into class tablee
	--this order allows overwriting builtin members
	for member,value in pairs(classdef) do
		class[member] = value
	end

	--resolve inheritance
	if class.__inherit then
		local parent = FinalClasses[class.__inherit]
		if parent then
			setmetatable(class, { __index = parent })
		else
			error(string.format("'%s' inherits non-existing class '%s'", name, class.__inherit))
		end
	else
		setmetatable(class, class)
	end

	return class
end

function BuildClasses()
	for name in pairs(FinalClasses) do
		rawset(_G, name, nil)
		if not ClassDefs[name] then
			rawset(FinalClasses, name, nil)
		end
	end

	for name in pairs(ClassDefs) do
		BuildSingleClass(name)
	end
end

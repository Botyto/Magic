require "XMLParser"

Class.CSharpParser = {
	__inherit = "XMLParser",
}

function CSharpParser:ParseFile(file)
	local lines = self:GetFileLines(file)
	local nlines = #lines
	local nodes = { }

	for i=1,nlines do
		lines[i] = string.trim_spaces(lines[i])
	end

	local last_class

	local i = 1
	while true do
		if i > nlines then break end
		local line = lines[i]

		if string.starts_with(line, "///") then
			local first = i
			local block = { }
			--read whole comment block
			while true do
				if i > nlines then break end
				local line = lines[i]

				if string.starts_with(line, "///") then
					table.insert(block, line)
				else
					break
				end

				i = i + 1
			end
			local last = i

			local node = self:ParseBlock(block)
			if node then
				self:SetupNode(node)
				if IsKindOf(node, "CodeNode") then
					node.file = file
					node.lines = { first = first, last = last }
				end
				if IsKindOf(node, "ClassNode") then
					last_class = node
				elseif IsKindOf(node, "CodeNode") then
					node.parent = last_class
					if IsKindOf(node, "MethodNode") then
						table.insert(last_class.methods, node)
					elseif IsKindOf(node, "PropertyNode") then
						table.insert(last_class.properties, node)
					elseif IsKindOf(node, "MessageNode") then
						table.insert(last_class.messages, node)
					end
				end
				table.insert(nodes, node)
			end
		end

		i = i + 1
	end

	return nodes
end

local function SkipWhitespaces(line, first)
	local i = first
	local len = #line

	while true do
		if i > len then return i end
		local chr = string.sub(line, i, i)
		if not IsWhitespace(chr) then break end

		i = i + 1
	end

	return i
end

local function ParseWord(line, word, first)
	local i = first
	local len = #line

	i = SkipWhitespaces(line, i)
	if i > len then return false, first end

	local ffirst, flast = string.find(line, word)
	if ffirst ~= i then
		return false, first
	end

	return true, flast + 1
end

local function PraseClassDefinition(line)
	local accessibility = "public"
	if string.find(line, "public") then accessibility = "public" end
	if string.find(line, "private") then accessibility = "private" end

	local class_first, class_last = string.find(line, "class ")
	local whitespace_pos = string.find(line, "%s", class_last + 1)
	local name = string.sub(line, class_last + 1, whitespace_pos - 1)

	local inherits
	local colon = string.find(line, ":")
	if colon then
		local inherits_line = string.sub(line, colon + 1)
		inherits = string.split(inherits_line, ",")
		for i=1,#inherits do
			inherits[i] = string.trim_spaces(inherits[i])
		end
	else
		inherits = { }
	end

	return accessibility, name, inherits
end

local function ParseMethodParameter(param)
	param = string.trim_spaces(param)
	if param == "" then return end

	local node = ParameterNode:new()

	local space = string.find(param, " ")
	local end_of_name = string.find(param, " ", space + 1) or string.find(param, "=", space + 1)
	node.value_type = string.sub(param, 1, space - 1)
	if end_of_name then
		node.title = string.sub(param, space + 1, end_of_name - 1)
	else
		node.title = string.sub(param, space + 1)
	end

	local equals = string.find(param, "=", space + 1)
	if equals then
		node.default = string.trim_spaces(string.sub(param, equals + 1))
	end

	return node
end

local function PraseMethodDefinition(line)
	local accessibility = "private"
	if string.find(line, "public") then accessibility = "public" end
	if string.find(line, "private") then accessibility = "private" end

	local static = not not string.find(line, "static")
	local virtual = not not string.find(line, "virutal")
	local abstract = not not string.find(line, "abstract")

	local name = name

	local bracket_open = string.find(line, "%(")
	local bracket_close = string.find(line, ")")
	local params_line = string.sub(line, bracket_open + 1, bracket_close - 1)
	local parameters = string.split(params_line, ",")
	for i=1,#parameters do
		parameters[i] = ParseMethodParameter(parameters[i])
	end

	local results = { }

	return accessibility, static, name, parameters, results
end

local function PrasePropertyDefinition(line)
	local accessibility = "private"
	if string.find(line, "public") then accessibility = "public" end
	if string.find(line, "private") then accessibility = "private" end

	local static = not not string.find(line, "static")

	local name = line

	local value_type = "int"
	local default = "0"

	return accessibility, static, name, value_type, default
end

function CSharpParser:ParseBlock(block)
	local nblock = #block

	--remove the "/// "
	for i=1,nblock do
		block[i] = string.sub(block[i], 5)
	end

	local node

	local text = table.concat(block, "\n")
	local elements = self:ParseXML(text)
	if elements then
		if elements.class then --node is class
			local accessibility, name, inherits = PraseClassDefinition(elements.class.content)
			if not accessibility then return end
			node = ClassNode:new()
			node.title = name
			node.accessibility = accessibility
			node.variants = { elements.class.content }
			node.inherits = inherits
			node.methods = { }
			node.properties = { }
			node.messages = { }
		elseif elements.method then --node is method
			local accessibility, static, name, parameters, results = PraseMethodDefinition(elements.method.content)
			if not accessibility then return end
			node = MethodNode:new()
			node.title = name
			node.accessibility = accessibility
			node.static = static
			node.variants = { elements.method.content }
			node.title = elements.method.content
			node.parameters = { parameters }
			node.results = { results }
		elseif elements.property then --node is property
			local accessibility, static, name, value_type, default = PrasePropertyDefinition(elements.property.content)
			if not accessibility then return end
			node = PropertyNode:new()
			node.title = name
			node.accessibility = accessibility
			node.static = static
			node.variants = { elements.property.content }
			node.title = elements.property.content
			node.value_type = value_type
			node.default = default
		end

		if node then
			node.summary = elements.summary and elements.summary.content

			node.tags = { }
			for i,elem in ipairs(elements) do
				if elem.tag == "tag" then
					table.insert(node.tags, elem.content)
				end
			end

			if IsKindOf(node, "CodeNode") then
				node.language = "csharp"

				node.examples = { }
				for i,elem in ipairs(elements) do
					if elem.tag == "example" then
						table.insert(node.examples, elem.content)
					end
				end
			end
		end
	end

	return node
end

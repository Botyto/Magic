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
				if IsKindOf(node, "CodeNode") then
					node.file = file
					node.lines = { first = first, last = last }
				end
				if IsKindOf(node, "ClassNode") then
					last_class = node
				elseif IsKindOf(node, "CodeNode") then
					node.parent = last_class
				end
				self:SetupNode(node)
				table.insert(nodes, node)
			end
		end

		i = i + 1
	end

	return nodes
end

local function PraseClassDefinition(line)
	local accessibility = "public"
	if string.find(line, "public") then accessibility = "public" end
	if string.find(line, "private") then accessibility = "private" end

	local class_first, class_last = string.find(line, "class ")
	if not class_first then return end --check syntax

	local name
	local whitespace_pos = string.find(line, "%s", class_last + 1)
	if whitespace_pos then
		name = string.sub(line, class_last + 1, whitespace_pos - 1)
	else
		name = string.sub(line, class_last + 1)
	end
	if not name or name == "" then return end --check syntax

	local inherits
	local colon = string.find(line, ":")
	if colon then
		local inherits_line = string.sub(line, colon + 1)
		inherits = string.split(inherits_line, ",")
		for i=1,#inherits do
			inherits[i] = string.trim_spaces(inherits[i])
		end
		if not next(inherits) then return end --check syntax
	else
		inherits = { }
	end

	return accessibility, name, inherits
end

local function ParseMethodParameter(param)
	param = string.trim_spaces(param)
	if param == "" then return end --check syntax

	local value_type, name, default

	local space = string.find(param, " ")
	value_type = string.sub(param, 1, space - 1)
	if not value_type or value_type == "" then return end --check syntax

	local end_of_name = string.find(param, " ", space + 1) or string.find(param, "=", space + 1)
	if end_of_name then
		name = string.sub(param, space + 1, end_of_name - 1)
	else
		name = string.sub(param, space + 1)
	end
	if not name or name == "" then return end --check syntax

	local equals = string.find(param, "=", space + 1)
	if equals then
		default = string.trim_spaces(string.sub(param, equals + 1))
		if not default or default == "" then return end --check syntax
	end

	local node = ParameterNode:new()
	node.value_type = value_type
	node.title = name
	node.default = default
	return node
end

local function PraseMethodDefinition(line)
	local public_first, public_last = string.find(line, "public ")
	local private_first, private_last = string.find(line, "private ")
	local accessibility = "private"
	if public_first then accessibility = "public" end
	if private_first then accessibility = "private" end

	local static_first, static_last = string.find(line, "static ")
	local virtual_first, virtual_last = string.find(line, "virutal ")
	local abstract_first, abstract_last = string.find(line, "abstract ")
	local static = not not static_first
	local virutal = not not virtual_first
	local abstract = not not abstract_first

	local before_type = math.max(static_last or 1, virutal_last or 1, abstract_last or 1, public_last or 1, private_last or 1)
	local type_last = string.find(line, " ", before_type + 1)
	if not type_last then return end --check syntax
	local result_type = string.sub(line, before_type + 1, type_last - 1)
	if not result_type or result_type == "" then return end --check syntax
	local only_result = ParameterNode:new()
	only_result.value_type = result_type
	only_result.title = result_type
	local results = { only_result }

	local name_end = string.find(line, "%(", type_last) or string.find(line, " ", type_last)
	if not name_end then return end --check syntax
	local name = string.trim_spaces(string.sub(line, type_last + 1, name_end - 1))

	local bracket_open = string.find(line, "%(")
	local bracket_close = string.find(line, ")")
	if not bracket_open or not bracket_close then return end --check syntax
	local params_line = string.sub(line, bracket_open + 1, bracket_close - 1)
	local parameters = string.split(params_line, ",")
	for i=1,#parameters do
		parameters[i] = ParseMethodParameter(parameters[i])
		if not parameters[i] then return end --check syntax
	end

	return accessibility, static, name, parameters, results
end

local function PrasePropertyDefinition(line)
	local public_first, public_last = string.find(line, "public ")
	local private_first, private_last = string.find(line, "private ")
	local accessibility = "private"
	if public_first then accessibility = "public" end
	if private_first then accessibility = "private" end

	local static_first, static_last = string.find(line, "static ")
	local static = not not static_first

	local before_type = math.max(static_last or 1, public_last or 1, private_last or 1)
	local type_last = string.find(line, " ", before_type + 1)
	if not type_last then return end --check syntax
	local type_name = string.sub(line, before_type + 1, type_last - 1)
	if not type_name or type_name == "" then return end --check syntax
	local value_type = ParameterNode:new()
	value_type.value_type = type_name

	local name, default
	local end_of_name = string.find(line, " ", type_last + 1) or string.find(line, "=", type_last + 1)
	if end_of_name then
		name = string.sub(line, type_last + 1, end_of_name - 1)
	else
		name = string.sub(line, type_last + 1)
	end
	if not name or name == "" then return end --check syntax

	local equals = string.find(line, "=", type_last + 1)
	if equals then
		default = string.trim_spaces(string.sub(line, equals + 1))
		if not default or default == "" then return end --check syntax
	end

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
			node.value_type = value_type
			node.default = default
		elseif elements.article then
			node = ArticleNode:new()
			node.title = elements.article.content
		end

		if node then
			node.summary = elements.summary and elements.summary.content

			for i,elem in ipairs(elements) do
				if elem.tag == "tag" then
					node:SetTag(elem.content, true)
				end
			end

			if IsKindOf(node, "CodeNode") then
				node.attributes = { }
				node.language = "csharp"

				node.examples = { }
				for i,elem in ipairs(elements) do
					if elem.tag == "example" then
						table.insert(node.examples, elem.content)
					end
				end
			end

			if IsKindOf(node, "PageNode") then
				node.article = elements.article and elements.article.content
			end
		end
	end

	return node
end

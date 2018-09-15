require "CodeNode"

Class.MethodNode = {
	__inherit = "CodeNode",

	parameters = false, --node parameters (per variant)
	results = false, --returned values (per variant)
}

function MethodNode:GeneratePageContent(writer)
	local page = { }

	for i=1,#self.variants do
		local variant = self.variants[i]
		local parameters = self.parameters[i]
		local results = self.results[i]
		table.insert(page, writer:GenerateSection("Description"))

		table.insert(page, writer:GenerateOrderedListLine("Syntax")) --1. syntax
		table.insert(page, writer:GenerateSnippet(variant, self.language, self.summary))

		if next(parameters or empty_table) then --2. parameters
			table.insert(page, writer:GenerateOrderedListLine("Parameters"))
			table.insert(page, self:GenerateParametersTable(writer, parameters))
		end

		if next(results or empty_table) then
			table.insert(page, writer:GenerateOrderedListLine("Results")) --3. results
			table.insert(page, self:GenerateParametersTable(writer, results))
		end

		writer:ResetOrderedList()
	end

	return page
end

function MethodNode:GenerateParametersTable(writer, list)
	local columns = { "Type", "Name", "Summary" }
	local t = { }
	for i,param in ipairs(list) do
		local type_text = IsKindOf(param.value_type, "PageNode") and param.value_type:GenerateLink(writer) or param.value_type
		table.insert(t, { type_text, param.title, param.summary or param.title })
	end

	return writer:GenerateTable(t, columns)
end

function MethodNode:TryMergeNode(other)
	if not IsKindOf(other, "MethodNode") then return end
	if other.parent ~= self.parent then return end
	if other.title ~= self.title then return end

	table.append(self.variants, other.variants)
	table.append(self.attributes, other.variants)
	table.append(self.parameters, other.parameters)
	table.append(self.results, other.results)
	table.append(self.examples, other.examples)

	return true
end

function MethodNode:UpdateConnectionsWithNode(other)
	CodeNode.UpdateConnectionsWithNode(self, other)

	for i,param_list in ipairs(self.parameters or empty_table) do
		for j,param in ipairs(param_list) do
			if IsKindOf(param, "Node") then
				param:UpdateConnectionsWithNode(other)
			end
		end
	end

	for i,result_list in ipairs(self.results or empty_table) do
		for j,result in ipairs(result_list) do
			if IsKindOf(result, "Node") then
				result:UpdateConnectionsWithNode(other)
			end
		end
	end
end

----------------------------

Class.ParameterNode = {
	__inherit = "Node",

	value_type = false, --value type (node, string or `false`)
	default = false, --default value (string or `false`)
}

function ParameterNode:GenerateContent(writer)
	return ""
end

function ParameterNode:UpdateConnectionsWithNode(other)
	self:TryUpdateConnectionMember("value_type", other)
end
